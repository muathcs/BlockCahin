# Blockchain Simulator

### Overview
This project is a **Blockchain Simulator** developed in **C#**, designed to mimic the essential functionality of a blockchain network. It provides a sandbox environment for simulating key blockchain components such as blocks, transactions, consensus mechanisms, cryptographic hashing, and network communication. The simulator serves as a useful tool for anyone interested in exploring blockchain concepts in a controlled, simplified setting without real-world risks or complexities.

### Features
- **Block Creation**: Simulates the creation of blocks, including linking blocks through cryptographic hashing to ensure data integrity.
- **Transaction Management**: Allows for the creation and verification of transactions within each block.
- **Consensus Mechanism**: Supports a consensus protocol to validate and secure the blockchain network, such as Proof-of-Work or Proof-of-Stake (depending on the implementation).
- **Cryptographic Hashing**: Uses secure hashing algorithms (e.g., SHA-256) to link blocks and ensure data immutability.
- **P2P Network Simulation**: Models basic peer-to-peer communication, enabling nodes to share and validate blocks across the simulated network.
- **Chain Verification**: Enables validation of the blockchain’s integrity by verifying each block's hash, transactions, and timestamps.


### Getting Started
#### Prerequisites
- **.NET SDK 5.0 or later**: Make sure you have the .NET SDK installed to build and run the project.

#### Installation
1. **Clone the Repository**:
   ```bash
   git clone https://github.com/muathcs/BlockCahin.git
   ```
2. **Navigate to Project Directory**:
   ```bash
   cd blockchain
   ```

#### Running the Simulator
1. **Build the Project**:
   ```bash
   dotnet build
   ```
2. **Run the Simulator**:
   ```bash
   dotnet run
   ```

### Usage
- **Add Transactions**: Use the console input to add transactions to blocks.
- **Mine Blocks**: Simulate mining with your chosen consensus mechanism to add blocks to the chain.
- **Inspect the Blockchain**: Print the chain data to verify its integrity and explore how each block is linked.

### Key Classes
- **Blockchain.cs**: Manages the chain, adding new blocks, and verifies block integrity.
- **Block.cs**: Defines the data structure and methods for each block, including cryptographic linkage to the previous block.
- **Transaction.cs**: Handles transaction creation, ensuring that each transaction has valid inputs and outputs.
- **Consensus.cs**: Defines the consensus logic, determining which node is allowed to add a new block.
- **NetworkSimulator.cs**: Mimics network behavior, allowing nodes to communicate block and transaction data.

### Examples
Below is a quick example of adding a transaction, mining a block, and viewing the current chain status:

```csharp
Blockchain blockchain = new Blockchain();
blockchain.AddTransaction(new Transaction("Alice", "Bob", 10.0));
blockchain.MineBlock("Miner1");
blockchain.PrintChain();
```

### Future Enhancements
- **Improved Consensus Algorithms**: Add other consensus mechanisms such as Delegated Proof-of-Stake.
- **Smart Contract Support**: Simulate basic smart contract execution on the blockchain.
- **Enhanced Network Simulation**: Include network latency and more realistic peer-to-peer behavior.

### License
This project is licensed under the MIT License.

---
