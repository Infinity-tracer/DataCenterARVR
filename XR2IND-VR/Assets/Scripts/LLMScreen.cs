using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using HuggingFace.API;
using TMPro;
using UnityEngine;
using System.Net;
using UnityEngine.InputSystem;
using UnityEngine.Networking;

public class LLMScreen : MonoBehaviour
{	
	[Header("LLM Server Configuration")]
	[SerializeField] private string llmServerUrl = "https://llm-chat-engine.onrender.com/api/chat";
	
	[Header("OpenAI Configuration")]
	// TODO: Load the OpenAI API key securely (e.g., from environment variable, config, or Unity secret manager)
	private const string openAIApiKey = ""; // DO NOT HARDCODE SECRETS
	private const string OPENAI_WHISPER_URL = "https://api.openai.com/v1/audio/transcriptions";
	
	[Header("UI References")]
	[SerializeField] private List<InputActionReference> toggleCanvas;
	[SerializeField] private List<InputActionReference> toggleRecording;
	[SerializeField] private InputActionReference controllerScroll;
	[SerializeField] private InputActionReference mouseScroll;
	[SerializeField] private List<InputActionReference> move;
	[SerializeField] private Canvas canvas;
	[SerializeField] private RectTransform textRect;
	[SerializeField] private RectTransform panel;
	[SerializeField] private RectTransform loadingRect;
	
	[Header("Settings")]
	public float moveSpeed = 10f;
	
	private TextMeshProUGUI text;
	private AudioClip recordingClip;
	private List<RectTransform> panelElements;
	public ARPanel[] canvases;
	private bool isCurrentlyRecording = false;
	private bool isMicrophoneReady = false;

	void Start()
	{
		// Force TLS 1.2 (essential for modern web requests in Unity)
		ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
		
		text = textRect.GetComponent<TextMeshProUGUI>();
		panelElements = new List<RectTransform> { textRect, loadingRect };
		ShowCanvas(false);
		
		foreach (var actionReference in toggleCanvas) 
		{
			if (actionReference != null)
				actionReference.action.performed += ToggleCanvas;
		}
		
		foreach (var actionReference in toggleRecording)
		{
			if (actionReference != null)
			{
				actionReference.action.performed += StartRecording;
				actionReference.action.canceled += StopRecording;
			}
		}
		
		CheckMicrophoneAvailability();
		
		Debug.Log("LLMScreen initialized successfully");
		Debug.Log("OpenAI API Key configured: " + (string.IsNullOrEmpty(openAIApiKey) ? "NO" : "YES"));
		Debug.Log($"LLM Server URL: {llmServerUrl}");
		
		// Run network diagnostic test
		StartCoroutine(TestNetworkConnectivity());
	}
	
	private IEnumerator TestNetworkConnectivity()
	{
		Debug.LogError("🔍 DIAGNOSTIC STARTED: Running network connectivity check...");
		
		// 1. Check DNS Resolution
		Debug.LogError($"🔍 Test 1: DNS Lookup for onrender.com");
		try
		{
			IPHostEntry host = Dns.GetHostEntry("llm-chat-engine.onrender.com");
			Debug.LogError($"✅ Test 1 PASSED: DNS Resolved to {host.AddressList[0]}");
		}
		catch (Exception e)
		{
			Debug.LogError($"❌ Test 1 FAILED: DNS Resolution Error: {e.Message}");
		}
		
		// 2. Test Google (Reachability)
		string googleUrl = "https://www.google.com";
		Debug.LogError($"🔍 Test 2: Reachability (Google)");
		
		using (UnityWebRequest googleTest = UnityWebRequest.Get(googleUrl))
		{
			googleTest.timeout = 10;
			yield return googleTest.SendWebRequest();
			
			if (googleTest.result == UnityWebRequest.Result.Success)
			{
				Debug.LogError($"✅ Test 2 PASSED: Google reachable");
			}
			else
			{
				Debug.LogError($"❌ Test 2 FAILED: Google unreachable: {googleTest.error}");
			}
		}
		
		// 3. Test LLM Server (GET)
		Debug.LogError($"🔍 Test 3: LLM Server Reachability ({llmServerUrl})");
		
		using (UnityWebRequest result = UnityWebRequest.Get(llmServerUrl))
		{
			// Headers mimicking a browser
			result.SetRequestHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
			result.SetRequestHeader("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*");
			
			result.timeout = 15;
			yield return result.SendWebRequest();
			
			// 405 Method Not Allowed is GOOD - means we reached the server!
			if (result.responseCode > 0)
			{
				Debug.LogError($"✅ Test 3 PASSED: Server Responded! Code: {result.responseCode}");
			}
			else
			{
				Debug.LogError($"❌ Test 3 FAILED: Connection Failed: {result.error}");
				Debug.LogError($"Error Details: {result.downloadHandler?.text}");
			}
		}
		
		Debug.LogError("🔍 DIAGNOSTIC COMPLETE");
	}

	private void CheckMicrophoneAvailability()
	{
		string[] devices = Microphone.devices;
		
		if (devices.Length == 0)
		{
			Debug.LogError("❌ No microphone detected!");
			UpdateText("❌ No microphone found. Please connect a microphone.");
			return;
		}
		
		Debug.Log($"✅ Found {devices.Length} microphone(s):");
		for (int i = 0; i < devices.Length; i++)
		{
			Debug.Log($"   [{i}] {devices[i]}");
		}
		
		isMicrophoneReady = true;
	}

	private void StartRecording(InputAction.CallbackContext ctx)
	{
		if (!isMicrophoneReady)
		{
			UpdateText("❌ Microphone not available");
			Debug.LogError("Cannot start recording - microphone not ready");
			return;
		}
		
		if (!isCurrentlyRecording)
		{
			Debug.Log("🎤 START RECORDING");
			
			Microphone.End(null);
			recordingClip = Microphone.Start(null, false, 10, 44100);
			
			if (recordingClip == null)
			{
				Debug.LogError("Failed to create recording clip!");
				UpdateText("❌ Failed to start recording");
				return;
			}
			
			isCurrentlyRecording = true;
			UpdateText("🎤 Recording... Speak now!");
			ShowElement(loadingRect);
			
			StartCoroutine(WaitForMicrophoneStart());
		}
	}

	private IEnumerator WaitForMicrophoneStart()
	{
		int timeout = 0;
		int maxTimeout = 100;
		
		while (Microphone.GetPosition(null) <= 0 && timeout < maxTimeout)
		{
			timeout++;
			yield return null;
		}
		
		if (timeout >= maxTimeout)
		{
			Debug.LogError("❌ Microphone failed to start after timeout");
			UpdateText("❌ Microphone initialization failed");
			isCurrentlyRecording = false;
			Microphone.End(null);
		}
		else
		{
			Debug.Log($"✅ Microphone ready after {timeout} frames");
		}
	}

	private void StopRecording(InputAction.CallbackContext ctx)
	{
		if (isCurrentlyRecording)
		{
			Debug.Log("⏹️ STOP RECORDING");
			StartCoroutine(ProcessRecording());
		}
	}

	private IEnumerator ProcessRecording()
	{
		yield return new WaitForSeconds(0.3f);
		
		if (recordingClip == null)
		{
			Debug.LogError("Recording clip is null!");
			UpdateText("❌ Recording error");
			isCurrentlyRecording = false;
			yield break;
		}
		
		int position = Microphone.GetPosition(null);
		int samples = recordingClip.samples;
		int channels = recordingClip.channels;
		int frequency = recordingClip.frequency;
		
		Debug.Log($"📊 Recording Info:");
		Debug.Log($"   Position: {position}");
		Debug.Log($"   Samples: {samples}");
		Debug.Log($"   Channels: {channels}");
		Debug.Log($"   Frequency: {frequency}");
		
		Microphone.End(null);
		isCurrentlyRecording = false;
		
		if (position <= 0)
		{
			Debug.LogError("❌ No audio recorded - microphone position is 0");
			UpdateText("❌ No audio recorded. Hold button for 2+ seconds and speak clearly.");
			ShowElement(textRect);
			yield break;
		}
		
		if (position > samples)
		{
			Debug.LogWarning($"⚠️ Position overflow: {position} > {samples}");
			position = samples;
		}
		
		float[] audioSamples = new float[position * channels];
		recordingClip.GetData(audioSamples, 0);
		
		Debug.Log($"📦 Extracted {audioSamples.Length} audio samples");
		
		bool hasAudio = false;
		float maxAmplitude = 0f;
		
		for (int i = 0; i < Mathf.Min(2000, audioSamples.Length); i++)
		{
			float amplitude = Mathf.Abs(audioSamples[i]);
			if (amplitude > maxAmplitude)
				maxAmplitude = amplitude;
			
			if (amplitude > 0.001f)
			{
				hasAudio = true;
			}
		}
		
		Debug.Log($"🔊 Max amplitude: {maxAmplitude}");
		
		if (!hasAudio || maxAmplitude < 0.001f)
		{
			Debug.LogWarning("⚠️ Audio recorded but appears silent or too quiet");
			UpdateText("⚠️ Audio too quiet. Speak louder and closer to microphone.");
			ShowElement(textRect);
			yield break;
		}
		
		byte[] wavData = EncodeAsWAV(audioSamples, frequency, channels);
		
		Debug.Log($"✅ Audio encoded to WAV: {wavData.Length} bytes ({wavData.Length / 1024f:F2} KB)");
		
		UpdateText("⏳ Sending to OpenAI Whisper...");
		ShowElement(textRect);
		
		StartCoroutine(SendToOpenAIWhisper(wavData));
	}

	private byte[] EncodeAsWAV(float[] samples, int frequency, int channels)
	{
		using (var memoryStream = new MemoryStream(44 + samples.Length * 2))
		{
			using (var writer = new BinaryWriter(memoryStream))
			{
				writer.Write("RIFF".ToCharArray());
				writer.Write(36 + samples.Length * 2);
				writer.Write("WAVE".ToCharArray());
				
				writer.Write("fmt ".ToCharArray());
				writer.Write(16);
				writer.Write((ushort)1);
				writer.Write((ushort)channels);
				writer.Write(frequency);
				writer.Write(frequency * channels * 2);
				writer.Write((ushort)(channels * 2));
				writer.Write((ushort)16);
				
				writer.Write("data".ToCharArray());
				writer.Write(samples.Length * 2);
				
				foreach (var sample in samples)
				{
					writer.Write((short)(sample * short.MaxValue));
				}
			}
			return memoryStream.ToArray();
		}
	}

	private IEnumerator SendToOpenAIWhisper(byte[] audioData)
	{
		Debug.Log($"📤 Sending {audioData.Length} bytes to OpenAI Whisper API");
		Debug.Log($"🔑 Using API key: {openAIApiKey.Substring(0, 20)}...");
		
		var formData = new List<IMultipartFormSection>
		{
			new MultipartFormFileSection("file", audioData, "recording.wav", "audio/wav"),
			new MultipartFormDataSection("model", "whisper-1"),
			new MultipartFormDataSection("language", "en")
		};
		
		UnityWebRequest request = UnityWebRequest.Post(OPENAI_WHISPER_URL, formData);
		request.SetRequestHeader("Authorization", "Bearer " + openAIApiKey);
		request.timeout = 30;
		
		yield return request.SendWebRequest();
		
		if (request.result == UnityWebRequest.Result.Success)
		{
			string jsonResponse = request.downloadHandler.text;
			Debug.Log("✅ OpenAI Response: " + jsonResponse);
			
			try
			{
				WhisperResponse response = JsonUtility.FromJson<WhisperResponse>(jsonResponse);
				
				if (!string.IsNullOrEmpty(response.text))
				{
					string transcribedText = response.text.Trim();
					Debug.Log($"🗣️ Transcribed: \"{transcribedText}\"");
					
					UpdateText("You: " + transcribedText);
					ShowElement(textRect);
					
					AskTheLLM(transcribedText);
				}
				else
				{
					Debug.LogWarning("⚠️ OpenAI returned empty text");
					UpdateText("You: (No speech detected in audio)");
					ShowElement(textRect);
				}
			}
			catch (Exception e)
			{
				Debug.LogError($"❌ Failed to parse OpenAI response: {e.Message}");
				Debug.LogError($"Response was: {jsonResponse}");
				UpdateText("❌ Failed to process speech recognition result");
				ShowElement(textRect);
			}
		}
		else
		{
			Debug.LogError($"❌ OpenAI API Error: {request.error}");
			Debug.LogError($"Response Code: {request.responseCode}");
			Debug.LogError($"Response: {request.downloadHandler.text}");
			
			string errorMessage = "Speech recognition failed";
			
			if (request.responseCode == 401)
			{
				errorMessage = "Invalid API key. Key is embedded in code but might be expired or invalid.";
			}
			else if (request.responseCode == 429)
			{
				errorMessage = "Rate limit exceeded. Wait a moment and try again.";
			}
			else if (request.error.Contains("timeout"))
			{
				errorMessage = "Request timeout. Check your internet connection.";
			}
			
			UpdateText($"❌ {errorMessage}");
			ShowElement(textRect);
		}
		
		request.Dispose();
	}

	public void AskTheLLM(string prompt)
	{
		Debug.Log($"🤖 Asking LLM: {prompt}");
		UpdateText($"You: {prompt}\n\n⏳ AI is thinking...\n(First request may take 30-60s if server is sleeping)");
		StartCoroutine(SendLLMRequest(prompt));
	}
	
	private IEnumerator SendLLMRequest(string prompt)
	{
		string jsonPayload = $@"{{
			""messages"": [
				{{
					""role"": ""user"",
					""content"": ""{EscapeJson(prompt)}""
				}}
			]
		}}";
		
		Debug.Log($"📤 Sending to LLM: {llmServerUrl}");
		Debug.Log($"📤 Payload: {jsonPayload}");
		
		// Use standard UnityWebRequest with using block for automatic disposal
		using (UnityWebRequest request = new UnityWebRequest(llmServerUrl, "POST"))
		{
			byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
			request.uploadHandler = new UploadHandlerRaw(bodyRaw);
			request.downloadHandler = new DownloadHandlerBuffer();
			
			// Set headers to mimic a browser
			request.SetRequestHeader("Content-Type", "application/json");
			request.SetRequestHeader("Accept", "application/json");
			request.SetRequestHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
			
			request.timeout = 60;
			
			Debug.Log("📡 Sending POST request...");
			yield return request.SendWebRequest();
			
			Debug.Log($"📊 Response Code: {request.responseCode}");
			
			if (request.result == UnityWebRequest.Result.Success)
			{
				string responseBody = request.downloadHandler.text;
				Debug.Log($"📥 Raw Response: {responseBody}");
				
				if (string.IsNullOrEmpty(responseBody))
				{
					NewLineTextAppend("AI: (Empty response)");
				}
				else if (responseBody.StartsWith("{"))
				{
					string aiResponse = ExtractResponseFromJson(responseBody);
					NewLineTextAppend("AI: " + aiResponse);
				}
				else
				{
					NewLineTextAppend("AI: " + responseBody);
				}
			}
			else
			{
				Debug.LogError($"❌ LLM Error: {request.error}");
				Debug.LogError($"Response Code: {request.responseCode}");
				Debug.LogError($"Response: {request.downloadHandler?.text}");
				
				string errorMessage = "Connection failed.";
				
				if (request.responseCode == 0)
				{
					errorMessage = $"Network error: {request.error}\n(Check Console for Diagnostics)";
				}
				else
				{
					errorMessage = $"Server Error {request.responseCode}";
				}
				
				NewLineTextAppend($"❌ AI Error: {errorMessage}");
			}
		}
	}

	private string ExtractResponseFromJson(string json)
	{
		string[] possibleFields = { "\"response\":", "\"message\":", "\"content\":", "\"text\":" };
		
		foreach (string field in possibleFields)
		{
			int startIndex = json.IndexOf(field);
			if (startIndex != -1)
			{
				startIndex += field.Length;
				
				while (startIndex < json.Length && (json[startIndex] == ' ' || json[startIndex] == '"'))
					startIndex++;
				
				int endIndex = json.IndexOf("\"", startIndex);
				if (endIndex != -1)
				{
					return json.Substring(startIndex, endIndex - startIndex);
				}
			}
		}
		
		return json;
	}

	private string EscapeJson(string text)
	{
		return text
			.Replace("\\", "\\\\")
			.Replace("\"", "\\\"")
			.Replace("\n", "\\n")
			.Replace("\r", "\\r")
			.Replace("\t", "\\t");
	}

	void Update()
	{
		Scroll();
		
		if (isCurrentlyRecording && recordingClip != null)
		{
			if (Time.frameCount % 60 == 0)
			{
				int pos = Microphone.GetPosition(null);
				Debug.Log($"📍 Recording... Position: {pos}");
			}
		}
	}

	public void OpenCanvas()
	{
		canvas.enabled = true;
		foreach (var actionReference in move)
		{
			if (actionReference != null)
				actionReference.action.Disable();
		}
		textRect.anchoredPosition3D = new Vector3(0, 0, 0);
	}

	public void CloseCanvas()
	{
		canvas.enabled = false;
		foreach (var actionReference in move)
		{
			if (actionReference != null)
				actionReference.action.Enable();
		}
	}

	public void ToggleCanvas(InputAction.CallbackContext ctx)
	{
		ShowCanvas(!canvas.enabled);
	}

	public void ShowCanvas(bool show)
	{
		if (show) OpenCanvas();
		else CloseCanvas();
	}

	public void UpdateText(string newText)
	{
		if (text != null)
		{
			text.text = newText;
			textRect.anchoredPosition3D = new Vector3(0, 0, 0);
			ShowElement(textRect);
		}
	}

	public void Scroll()
	{
		if (controllerScroll == null || mouseScroll == null)
			return;
			
		Vector2 scrollValue = Vector2.zero;
		
		if (controllerScroll.action.phase != InputActionPhase.Waiting)
		{
			scrollValue = controllerScroll.action.ReadValue<Vector2>();
		}
		else if (mouseScroll.action.phase != InputActionPhase.Waiting)
		{
			scrollValue = mouseScroll.action.ReadValue<Vector2>();
		}

		float offset = Time.deltaTime * moveSpeed * scrollValue.y;
		offset = mouseScroll.action.phase != InputActionPhase.Waiting ? offset / 10f : offset;

		float newYpos = textRect.anchoredPosition3D.y + offset;

		if (newYpos <= textRect.sizeDelta.y - panel.rect.height && newYpos >= 0)
		{
			textRect.anchoredPosition3D += new Vector3(0, offset, 0);
		}
	}

	public void ShowElement(RectTransform element)
	{
		foreach (var panelElement in panelElements)
		{
			if (panelElement != null)
			{
				panelElement.gameObject.SetActive(panelElement == element);
			}
		}
		ShowCanvas(true);
	}

	public void InlineTextAppend(string extraText)
	{
		if (text != null)
		{
			text.text += extraText;
		}
	}

	public void NewLineTextAppend(string extraText)
	{
		if (text != null)
		{
			text.text = text.text + "\n" + extraText;
		}
	}

	private void OnDestroy()
	{
		if (isCurrentlyRecording)
		{
			Microphone.End(null);
		}
	}
}

[System.Serializable]
public class WhisperResponse
{
	public string text;
}

// Certificate handler to bypass SSL validation in development
public class BypassCertificateHandler : CertificateHandler
{
	protected override bool ValidateCertificate(byte[] certificateData)
	{
		// Always accept certificates in development
		// WARNING: Only use in development, not production!
		return true;
	}
}