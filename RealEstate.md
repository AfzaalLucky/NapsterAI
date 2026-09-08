Web SDK = the AI interface/agent the customer interacts with.
EdgeMCP/WebMCP = the tools that let the AI actually use your real-estate application.
Backend APIs/MCP servers = access to your listings, CRM, DLD data, inventory, pricing, availability, etc.


Customer
   ↓
Website
   ↓
"Contact us"
   ↓
Lead enters CRM
   ↓
Sales agent calls
   ↓
Agent asks requirements
   ↓
Agent searches inventory
   ↓
Agent sends PDFs
   ↓
Customer asks about payment plan
   ↓
Agent checks another system
   ↓
Viewing
   ↓
Follow-up
   ↓
Booking

You can turn that into:

Customer
   ↓
AI Sales Agent
   │
   ├── Understands requirements
   ├── Searches live inventory
   ├── Recommends units
   ├── Calculates payment plan
   ├── Answers project questions
   ├── Books viewing
   ├── Creates CRM lead
   ├── Assigns sales agent
   └── Escalates when necessary
             ↓
        Human Sales Agent

This isn't hypothetical as a workflow: current real-estate agent deployments are already focusing on lead qualification, live inventory matching, CRM synchronization and viewing booking. 
P
PhantomCore
+1

2. Where Web SDK fits
I'd separate the system into three layers.

                 CUSTOMER
                    │
                    ▼
        ┌──────────────────────┐
        │    Web SDK / UI      │
        │                      │
        │  Voice               │
        │  Chat                │
        │  AI assistant        │
        └──────────┬───────────┘
                   │
                   ▼
        ┌──────────────────────┐
        │     AI AGENT         │
        │                      │
        │ reasoning            │
        │ qualification        │
        │ recommendations      │
        │ conversation         │
        └──────────┬───────────┘
                   │
                   ▼
        ┌──────────────────────┐
        │       WebMCP         │
        │   Business Actions   │
        └──────────┬───────────┘
                   │
       ┌───────────┼────────────┐
       ▼           ▼            ▼
    Inventory     CRM        Booking
       │           │            │
       ▼           ▼            ▼
    ERP/API     Salesforce   Calendar

1. A real-estate architecture
I'd build it roughly like this:

                    CUSTOMER
                       │
                       ▼
              ┌─────────────────┐
              │  Web SDK / AI   │
              │  Assistant      │
              └────────┬────────┘
                       │
                       ▼
              ┌─────────────────┐
              │ WebMCP /        │
              │ Edge MCP        │
              │ Agent Tools     │
              └────────┬────────┘
                       │
          ┌────────────┼─────────────┐
          ▼            ▼             ▼
    Property API     CRM API      Backend
          │            │             │
          ▼            ▼             ▼
     Listings       Leads        Business Logic
          │
          ▼
   ┌─────────────────────┐
   │ External data       │
   │ DLD / portals / GIS │
   │ market data etc.    │
   └─────────────────────┘