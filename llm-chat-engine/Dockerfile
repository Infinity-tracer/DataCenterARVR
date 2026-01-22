FROM python:3.11-slim

WORKDIR /app

COPY . /app

# Install Poetry
RUN pip install poetry

# Configure Poetry:
# - Do not create a virtual environment inside the container
# - Install only package dependencies (skip dev-dependencies)
RUN poetry config virtualenvs.create false
RUN poetry install --only main -vvv
RUN pip install llama_index

# Create vector_store directory
RUN mkdir -p /app/vector_store

# OpenAI API key and other secrets should be set via Render environment variables
# Do NOT hardcode API keys in this file

EXPOSE 10000

# Command to run the FastAPI application
# Render sets the PORT environment variable
CMD uvicorn main:app --host 0.0.0.0 --port ${PORT:-10000}
