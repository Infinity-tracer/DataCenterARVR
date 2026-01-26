# LLM-Driven Industrial Maintenance Chatbot Assistant 
## Project Description
This project is designed to develop an AI-driven chatbot specialized in Industrial 5.0 maintenance practices, including
machinery optimization and process efficiency. The chatbot leverages OpenAI's GPT model for natural 
language understanding and responses, along with specialized tools for querying maintenance manuals and 
documents.

## Key Features
 - **Industrial Maintenance Expertise:** The chatbot is tailored to provide precise and accurate responses related to industrial machinery maintenance and process efficiency.
 - **Document Upload and Indexing:** Users can upload maintenance manuals and documents, which are then processed and indexed for querying.
 - **Contextual Conversation:** Maintains context from previous interactions to ensure coherent and relevant responses.
 - **Real-time Streaming Responses:** Provides real-time streaming of responses for a seamless user experience.

## Files and Directories
 - main.py: Entry point for the application.
 - Dockerfile: Docker configuration for containerizing the application.
 - pyproject.toml: Configuration file for managing project dependencies and metadata.
 - app/
   - api/
     - routers/
       - chat.py: Defines the API routes for interacting with the chat engine.
       - upload.py: API routes for uploading and processing documents.
   - engine/
     - context.py: Contains functions for setting up the service context, including language models and embedding models.
     - constants.py: Defines constants used throughout the application.
     - index.py: Handles the creation and management of the OpenAIAgent and its tools, including the chat engine.

## Installation and Setup

1. Navigate to the project directory:
```bash
cd llm-chat-engine
```

2. Put your OpenAI API key in the Dockerfile and docker-compose.yml

3. Build the Docker image:
```bash
docker build -t llm-engine .
```

5. Run the Docker service:

```bash
docker-compose up
```



## Deployment

The LLM Engine is deployed on **Render Cloud**. API endpoints can be accessed via the public URL provided by Render.

### Deploying to Render
1. **Create a New Web Service**:
   - Go to your [Render Dashboard](https://dashboard.render.com/).
   - Click **New +** and select **Web Service**.
   - Connect your GitHub repository (`llm-chat-engine`).

2. **Configure Settings**:
   - **Name**: Give your service a name (e.g., `llm-chat-engine`).
   - **Region**: Select a region close to your users (e.g., Oregon, Frankfurt).
   - **Branch**: Select the branch to deploy (usually `main`).
   - **Runtime**: Select **Docker**.

3. **Environment Variables**:
   Add the following environment variables in the "Environment" tab:
   - `OPENAI_API_KEY`: **Required**. Your OpenAI API key (starts with `sk-...`).
   - `PORT`: `8000` (Optional, Render usually detects this from the Dockerfile).

4. **Deploy**:
   - Click **Create Web Service**.
   - Render will build the Docker image and deploy it. This may take a few minutes.
   - Once live, your URL will look like: `https://<your-app-name>.onrender.com`.

## Usage

### Local Usage

To run locally, follow the **Installation and Setup** steps above.

### Cloud Usage

You can interact with the deployed API by replacing `http://localhost:8000` with your Render application URL (e.g., `https://your-app-name.onrender.com`).

## API Usage Examples

### 1. Unity Client Integration

The Unity client (`LLMScreen.cs`) primarily uses the `/api/chat` endpoint.

#### Chat Completion (POST)
**Endpoint:** `/api/chat`
**Method:** `POST`
**Description:** Sends a user message to the LLM and receives a streaming response.

```bash
curl -X POST "https://<your-app>.onrender.com/api/chat" \
  -H "Content-Type: application/json" \
  -d '{
    "messages": [
      {
        "role": "user",
        "content": "Hello, how do I maintain this machine?"
      }
    ]
  }'
```

#### Connectivity Check (GET)
**Endpoint:** `/api/chat`
**Method:** `GET`
**Description:** Used by the Unity client to verify server reachability.
**Expected Response:** `405 Method Not Allowed` (This confirms the server is reachable and the endpoint exists, even though GET is not explicitly handled).

### 2. Document Upload (Management)

Upload manuals for the RAG engine.

```bash
# Render Cloud Example
curl -X POST "https://<your-app>.onrender.com/api/upload/upload_document/" \
  -H "accept: application/json" \
  -H "Content-Type: multipart/form-data" \
  -F "doc_type=manual" \
  -F "manufacturer=cisco" \
  -F "doc_title=example_series" \
  -F "uploaded_file=@/path/to/your/document.pdf"
```

### 3. Swagger UI

Open [http://localhost:8000/api/docs](http://localhost:8000/api/docs) (or your Render URL equivalent) to view the interactive API documentation.

The API allows CORS for all origins to simplify development. You can change this behavior by setting the `ENVIRONMENT` environment variable to `prod`.

## Learn More

To learn more about LlamaIndex, take a look at the following resources:

- [LlamaIndex Documentation](https://docs.llamaindex.ai) - learn about LlamaIndex.

You can check out [the LlamaIndex GitHub repository](https://github.com/run-llama/llama_index) - your feedback and contributions are welcome!



