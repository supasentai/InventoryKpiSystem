# Inventory KPI Calculation System

This project implements a backend inventory analytics system that processes JSON invoices and purchase orders to compute key inventory KPIs.

The system is built using **C# (.NET 8)** and demonstrates concepts from **Systems Programming**, including asynchronous processing, file monitoring, and memory-efficient data handling.

---

# Team Development Workflow

To ensure smooth collaboration between team members, we follow a structured Git workflow.

Members **must NOT commit directly to the `main` branch**.

All development must be done through feature branches.

---

# Repository Setup

Clone the repository:
git clone https://github.com/supasentai/InventoryKpiSystem.git

Enter the project directory:
cd InventoryKpiSystem

---

# Branching Strategy

We use a **feature branch workflow**.

Example branches:
main (stable code)

feature/models
feature/json-loader
feature/kpi-engine
feature/file-watcher
feature/reporting

Each developer works on their own feature branch.

---

# Development Workflow

### 1 Create a new branch

Before starting a task:
git checkout -b feature/your-feature-name

Example:
git checkout -b feature/json-loader

---

### 2 Implement your feature

Write code only related to the assigned task.

Make sure the project still builds successfully.

---

### 3 Commit your changes
git add .
git commit -m "Implement JSON loader"

Use meaningful commit messages.

---

### 4 Push your branch
git push origin feature/your-feature-name

Example:
git push origin feature/json-loader

---

### 5 Create a Pull Request

Go to the GitHub repository and create a **Pull Request** from your branch to `main`.

The team leader will review the code before merging.

---

# Code Review Rules

Before merging a Pull Request:

- Code must compile successfully
- No unnecessary commented code
- Follow C# naming conventions
- Methods must have clear responsibilities

---

# Project Timeline

### Phase 1

Project setup

- Repository setup
- Project structure
- Data models
- JSON parsing

### Phase 2

Core system development

- Inventory state management
- KPI calculation engine

### Phase 3

Real-time data processing

- File monitoring
- Asynchronous processing queue

### Phase 4

System completion

- Duplicate detection
- KPI reporting
- Testing and debugging

---

# Contribution Guidelines

All team members should:

- Work only on assigned issues
- Create a branch for each task
- Submit Pull Requests for review
- Write clear commit messages

---

# Expected Output

The system will generate KPI results such as:

- Total SKUs
- Inventory Value
- Out-of-Stock Items
- Average Daily Sales
- Average Inventory Age

Results will be displayed in the console and exported as JSON reports.

---

# Technologies Used

- C#
- .NET 8
- System.Text.Json
- FileSystemWatcher
- LINQ
- Async/Await
- Producer–Consumer Pattern

---

# Goal of the Project

The purpose of this assignment is to practice:

- File processing
- Asynchronous programming
- Efficient data handling
- Real-time system design
