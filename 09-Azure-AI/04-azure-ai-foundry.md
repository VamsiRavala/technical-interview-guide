# Azure AI Foundry — The AI Application Platform

> Azure AI Foundry (formerly Azure AI Studio) is Microsoft's unified platform for **building, evaluating, deploying, and governing** AI applications and agents. Azure OpenAI is one service *inside* Foundry; Foundry is the control plane that ties model selection, prompt engineering, evaluation, safety, and LLMOps together.

---

## Azure OpenAI vs Azure AI Foundry (don't conflate)

| | **Azure OpenAI Service** | **Azure AI Foundry** |
|---|---|---|
| What it is | The model-serving **service** (deployments, inference APIs, content filters, PTUs) | The end-to-end **platform / experience** for building & operating AI apps and agents |
| Scope | OpenAI models only | **Model catalog** (OpenAI **+** Llama, Mistral, Phi, etc.), playground, prompt flow, evaluation, On Your Data, **Agent Service**, content safety, fine-tuning, monitoring, project/hub governance & connections |

In one line: **Azure OpenAI = model serving; Azure AI Foundry = the platform that includes Azure OpenAI among many capabilities.** You *consume* Azure OpenAI models but *build, evaluate, secure, and operate* in Foundry (with the **Microsoft Agent Framework** SDK for custom orchestration).

---

## Foundry Hub & Project Structure

```text
Foundry Hub  (shared security, networking, connections, compute, policy)
  ├─ Project A  (model deployments, prompt flows, eval runs, indexes, agents)
  ├─ Project B
  └─ Connections: Azure OpenAI, AI Search, Storage, Key Vault, App Insights
```

- The **hub** centralizes governance, networking (BYO VNet/storage), and **connections** to dependent Azure services.
- **Projects** are the working units where you build flows, deploy models, run evals, and host agents.
- Access is via **Entra ID + RBAC**; secrets via **Key Vault** connections.

---

## Model Catalog

A browsable catalog spanning:
- **Azure OpenAI** models (GPT-4o, 4.1, o-series, embeddings),
- **Open / partner models** (Meta Llama, Mistral, Microsoft **Phi**, etc.) deployable as **managed online endpoints** or serverless APIs,
- model cards with benchmarks, license, and intended use.

This lets you pick the right model per task (and per cost/latency budget) and even **route** across providers, all under one governance plane.

---

## Prompt Flow & Evaluation

**Prompt Flow** is Foundry's visual + code orchestration tool for building and operationalizing LLM workflows (chains): nodes for prompts, Python tools, model calls, and connections, wired into a DAG you can run, debug, and deploy. It makes **chaining explicit and observable** and integrates with version control.

**Evaluation** (the LLMOps backbone) — built-in evaluators you run on datasets and in CI:
- **Quality**: groundedness, relevance, coherence, fluency, similarity.
- **RAG triad**: context relevance, groundedness, answer relevance (+ retrieval metrics).
- **Safety/risk**: hate, violence, self-harm, sexual, protected material, jailbreak.
- **Agent/trajectory** evaluators for multi-step agents.

The Foundry **evaluation SDK** runs these programmatically so you can **gate releases** on score thresholds and catch regressions when you change chunking, prompts, or model versions. Also includes **red-teaming** tooling (automated adversarial agent / PyRIT) to probe for jailbreaks and misuse.

Microsoft's RAI loop maps directly onto these tools: **identify → measure → mitigate → operate** (see `05-content-safety-and-responsible-ai.md`).

---

## Foundry Agent Service

A **managed, stateful agent runtime** so you don't build the orchestration infrastructure yourself. (Evolution of the Assistants API. Verify current GA status — naming/features in this area move fast.)

You define an **agent** with instructions, a model, and **tools**:
- **Built-in tools**: **file search** (managed RAG), **code interpreter** (sandboxed code execution), **Bing grounding**, function/OpenAPI tools, and **MCP** connectors.
- The service manages **threads** (conversation state), tool invocation, and execution **server-side**.
- Enterprise features: **identity/RBAC**, **networking** (BYO VNet/storage/search), **content safety**, **observability/tracing**, **evaluation**, and **multi-agent** orchestration.

### Agent Service vs Semantic Kernel (complementary)

| | **Semantic Kernel (SK)** | **Foundry Agent Service** |
|---|---|---|
| Form | Open-source **SDK** embedded in your app | Managed **cloud service** that hosts agents |
| Control | Full in-process control (great for .NET) | Less code, managed infra + built-in tools |
| State | You manage | Managed **threads** server-side |
| Tools | Plugins / functions you write | Built-in (code interpreter, file search, Bing) + your tools + MCP |

They converge under the **Microsoft Agent Framework** (the AutoGen + SK convergence). Common pattern: build agent logic with the **MAF/SK .NET SDK**, host on the **Foundry Agent Service** for managed state, tools, and governance. See the dedicated Semantic Kernel and AI-Agents sections for depth.

> Choose SK when you want lightweight, embeddable orchestration and control; choose the Agent Service when you want a hosted runtime with built-in tools and Azure-grade governance. Many solutions do both.

---

## Where Foundry Fits in the Lifecycle

```text
Pick model (catalog) ─► Build flow / agent (Prompt Flow / Agent Service / SK)
   ─► Evaluate (quality + safety + groundedness, in CI)
   ─► Red-team ─► Deploy (endpoint / agent) ─► Monitor (App Insights + tracing) ─► iterate
```

It's the place to run the whole loop instead of stitching disparate tools — pick a model, build and evaluate a flow, run safety evals, deploy, and observe.

---

## Brush-Up Cheat-Sheet

- **Azure OpenAI = model serving; Foundry = the platform** (catalog + prompt flow + eval + agents + safety + governance).
- **Hub/Project** structure: hub = shared security/networking/connections; project = where you build.
- **Model catalog** = OpenAI + open/partner models (Llama, Mistral, Phi) under one governance plane.
- **Evaluation SDK** = groundedness/relevance/safety/agent evaluators, **run in CI to gate releases**; plus **red-teaming**.
- **Foundry Agent Service** = managed stateful agents with built-in tools (file search, code interpreter, Bing, MCP); complements **Semantic Kernel / Microsoft Agent Framework**. *(verify current GA status)*
