# AI / RAG Agent
# Scope: Azure OpenAI, AI Search, RAG pipelines, Content Safety

Follow AGENTS.md as primary source.

## Azure AI Resource Naming
| Resource              | Pattern                        | Example                    |
|-----------------------|--------------------------------|----------------------------|
| Azure OpenAI          | oai-<workload>-<env>-<###>     | oai-hcw-prod-001           |
| AI Search             | srch-<workload>-<env>          | srch-hcw-prod              |
| Content Safety        | cs-<workload>-<env>-<###>      | cs-hcw-prod-001            |
| ML Workspace          | mlw-<workload>-<env>-<###>     | mlw-hcw-prod-001           |
| Document Intelligence | di-<workload>-<env>-<###>      | di-hcw-prod-001            |
| Bot Service           | bot-<workload>-<env>           | bot-hcw-prod               |

## RAG Architecture Standards
- Chunking: 512 tokens overlap 128 — document in index schema
- Embedding model: text-embedding-3-large (3072 dims) unless size-constrained
- Retrieval: hybrid (keyword + vector) with semantic reranker
- Index schema: include `content`, `embedding`, `source_url`, `tenant_id`, `created_at`
- Tenant isolation: filter on `tenant_id` field — never cross-tenant retrieval

## Content Safety
- Enable on all user-facing OpenAI endpoints
- Minimum thresholds: Hate=2, Violence=2, Sexual=0, SelfHarm=2
- Log all filtered requests to Log Analytics workspace

## Prompt Engineering Rules
- System prompts: stored in Key Vault or App Configuration — not hardcoded
- Max context: leave 20% token buffer for response
- Temperature: 0.0–0.3 for factual/RAG, 0.7–1.0 for creative
- Always include citation instructions in RAG system prompts

## Model Deployment Naming
```
<model-family>-<version>-<purpose>
gpt4o-2024-11-20-chat
gpt4o-mini-2024-07-18-rag
embedding-3-large-001
```
