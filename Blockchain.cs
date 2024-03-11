using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockchainAssignment
{
    class Blockchain
    {
        // List of block objects forming the blockchain
        public List<Block> blocks;

        // Maximum number of transactions per block
        private int transactionsPerBlock = 11116;

        // List of pending transactions to be mined
         public List<Transaction> transactionPool = new List<Transaction>();

        private List<StakingParticipant> stakingParticipants = new List<StakingParticipant>();


        // Default Constructor - initialises the list of blocks and generates the genesis block
        public Blockchain()
        {
            blocks = new List<Block>()
            {
                new Block() // Create and append the Genesis Block
            };
        }

        // Prints the block at the specified index to the UI
        public String GetBlockAsString(int index, string method = "default")
        {
            // Check if referenced block exists
            if (index >= 0 && index < blocks.Count)
                return blocks[index].ToString(method); // Return block as a string
            else
                return "No such block exists";
        }


        // Proof of Stake block creation
        public void CreateProofOfStakeBlock(string minerAddress)
        {
            // Check if the miner is a valid participant
            StakingParticipant miner = stakingParticipants.FirstOrDefault(p => p.Address == minerAddress);

            if (miner != null && miner.Stake >= 1) // Assuming 1 as the minimum stake required
            {
                Console.WriteLine($"Proof of Stake: Miner {miner.Address} with stake {miner.Stake} creates a new block.");

                // Get pending transactions and remove from the pool
                List<Transaction> transactions = GetPendingTransactions();

                // Create a new block with Proof of Stake
                Block newBlock = new Block(GetLastBlock(), transactions, minerAddress, "default");
                blocks.Add(newBlock);
            }
            else
            {
                Console.WriteLine("Proof of Stake: Invalid miner or insufficient stake to create a block.");
            }
        }

        // Add a participant to the Proof of Stake system
        public void AddStakingParticipant(StakingParticipant participant)
        {
            stakingParticipants.Add(participant);
        }

        // Print the list of staking participants
        public void PrintStakingParticipants()
        {
            Console.WriteLine("Staking Participants:");
            foreach (var participant in stakingParticipants)
            {
                Console.WriteLine($"{participant.Address} - Stake: {participant.Stake}");
            }
        }
    

    // Staking participant class
    public class StakingParticipant
    {
        public string Address { get; private set; }
        public double Stake { get; private set; }

        public StakingParticipant(string address, double initialStake)
        {
            Address = address;
            Stake = initialStake;
        }
    }




    // Retrieves the most recently appended block in the blockchain
    public Block GetLastBlock()
        {
            return blocks[blocks.Count - 1];
        }

        // Retrieve pending transactions and remove from pool
        public List<Transaction> GetPendingTransactions()
        {
            // Determine the number of transactions to retrieve dependent on the number of pending transactions and the limit specified
            int n = Math.Min(transactionsPerBlock, transactionPool.Count);

            // "Pull" transactions from the transaction list (modifying the original list)
            List<Transaction> transactions = transactionPool.GetRange(0, n);
            transactionPool.RemoveRange(0, n);

            // Return the extracted transactions
            return transactions;
        }

        // 
        // Greedy selection, get highest fee first
        public void GreedyTransactionSelection()
        {
            transactionPool = transactionPool.OrderByDescending(t => t.fee).ToList();


        }

        // Check validity of a blocks hash by recomputing the hash and comparing with the mined value
        public static bool ValidateHash(Block b)
        {
            String rehash = b.CreateHash();
            return rehash.Equals(b.hash);
        }

        // Check validity of the merkle root by recalculating the root and comparing with the mined value
        public static bool ValidateMerkleRoot(Block b)
        {
            String reMerkle = Block.MerkleRoot(b.transactionList);
            return reMerkle.Equals(b.merkleRoot);
        }

        // Check the balance associated with a wallet based on the public key
        public double GetBalance(String address)
        {
            // Accumulator value
            double balance = 0;

            // Loop through all approved transactions in order to assess account balance
            foreach(Block b in blocks)
            {
                foreach(Transaction t in b.transactionList)
                {
                    if (t.recipientAddress.Equals(address))
                    {
                        balance += t.amount; // Credit funds recieved
                    }
                    if (t.senderAddress.Equals(address))
                    {
                        balance -= (t.amount + t.fee); // Debit payments placed
                    }
                }
            }
            return balance;
        }

        // Output all blocks of the blockchain as a string
        public override string ToString()
        {
            return String.Join("\n", blocks);
        }
    }
}
