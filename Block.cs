using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace BlockchainAssignment
{
    class Block
    {
        /* Block Variables */
        private DateTime timestamp; // Time of creation

        private int index = 2; // Position of the block in the sequence of blocks

        public String prevHash, // A reference pointer to the previous block
            hash, // The current blocks "identity"
            merkleRoot,  // The merkle root of all transactions in the block
            minerAddress; // Public Key (Wallet Address) of the Miner

        public List<Transaction> transactionList; // List of transactions in this block
        
        // Proof-of-work
        public long nonce; // Number used once for Proof-of-Work and mining

        // Rewards
        public double reward; // Simple fixed reward established by "Coinbase"

        public TimeSpan totalTimeToMine;


        private static SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

        // Shared result variables
        private static bool matchFound = false;
        private static string matchingHash;
        private static long matchingNonce;





        // New property to store the target block time (in seconds)
        private double targetBlockTime = 5;
        double blockTime;
        private static int difficulty = 5; // An arbitrary number of 0's to proceed a hash value
        // New method to adjust difficulty dynamically
        public void AdjustDifficulty(Block lastBlock)
        {
            if (lastBlock != null )
            {
                blockTime = (timestamp - lastBlock.timestamp).TotalSeconds;

                // Adjust difficulty based on the target block time
                if (blockTime > targetBlockTime)
                {
                    difficulty-=1; //decrease difficulty 
                }
                else if(blockTime < targetBlockTime)
                {
                     
                    difficulty+= 1;//Increase difficulty
                }
                // Ensure difficulty doesn't go below 1
                difficulty = Math.Max(1, difficulty);
            }
        }

        /* Genesis block constructor */
        public Block()
        {
            timestamp = DateTime.Now;
            index = 0;
            transactionList = new List<Transaction>();
            hash = MineWithMultipleThreads();
        }

        /* New Block constructor */
        public Block(Block lastBlock, List<Transaction> transactions, String minerAddress, string transactionSelectionStrategy = "default")
        {
            timestamp = DateTime.Now;

            index = lastBlock.index + 1;
            prevHash = hash;



            // Apply transaction selection strategy
            if (transactionSelectionStrategy == "greedy")
            {
                transactions = transactions.OrderByDescending(t => t.fee).ToList();
            }
            else if (transactionSelectionStrategy == "altruistic")
            {
                transactions = transactions.OrderBy(t => t.timestamp).ToList();
            }
            else if (transactionSelectionStrategy == "random")
            {
                Random random = new Random();
                transactions = transactions.OrderBy(t => random.Next()).ToList();
            }


            this.minerAddress = minerAddress; // The wallet to be credited the reward for the mining effort
            reward = 1.0; // Assign a simple fixed value reward
            transactions.Add(createRewardTransaction(transactions)); // Create and append the reward transaction
            transactionList = new List<Transaction>(transactions); // Assign provided transactions to the block


            //adjust difficulty dynamically
            AdjustDifficulty(lastBlock);

            merkleRoot = MerkleRoot(transactionList); // Calculate the merkle root of the blocks transactions
            hash = MineWithMultipleThreads(); // Conduct PoW to create a hash which meets the given difficulty requirement
        }

        // Method for parallelized Proof-of-Work
        public string MineWithMultipleThreads()
        {
            int numberOfThreads = Environment.ProcessorCount; // Use the number of available cores
            Task<string>[] mineTasks = new Task<string>[numberOfThreads];
            
            
            
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();


            for (int i = 0; i < numberOfThreads; i++)
            {
                int threadIndex = i; // Capture the variable to avoid closure issues

                mineTasks[i] = Task.Run(() =>
                {
                    return MineWithThreadIndex(threadIndex);
                });
            }

            stopwatch.Stop();

            // Get the elapsed time
            totalTimeToMine = stopwatch.Elapsed;

            // Wait for any task to complete
            int completedTaskIndex = Task.WaitAny(mineTasks);

            // Signal other tasks to stop
            matchFound = true;

            // Retrieve the result from the completed task
            string resultHash = mineTasks[completedTaskIndex].Result;

            // Update the block properties with the matching result
            hash = resultHash;
            nonce = matchingNonce;

            return resultHash;
        }

        // Method for each thread to perform Proof-of-Work with a specific index
        private string MineWithThreadIndex(int threadIndex)
        {
            long startNonce = threadIndex;
            long step = Environment.ProcessorCount;


            //    Stopwatch stopwatch = new Stopwatch();
            //stopwatch.Start();

            while (!matchFound)
            {
                string hash = CreateHash(startNonce);
                if (hash.StartsWith(new string('0', difficulty)))
                {
                    // If a match is found, acquire semaphore and update shared variables
                    semaphore.Wait();
                    if (!matchFound)
                    {
                        matchFound = true;
                        matchingHash = hash;
                        matchingNonce = startNonce;
                    }
                    semaphore.Release();

                    return hash;
                }

                startNonce += step;
            }

            // Stop the stopwatch
            //stopwatch.Stop();

            //// Get the elapsed time
            //totalTimeToMine = stopwatch.Elapsed;

            // Print the total elapsed time
            //Console.WriteLine($"Total time taken: {temestamp}");

            return null; // Not reached
        }

        /* Hashes the entire Block object */
        public String CreateHash()
        {
            String hash = String.Empty;
            SHA256 hasher = SHA256Managed.Create();

            /* Concatenate all of the blocks properties including nonce as to generate a new hash on each call */
            String input = timestamp.ToString() + index + prevHash + nonce + merkleRoot;

            /* Apply the hash function to the block as represented by the string "input" */
            Byte[] hashByte = hasher.ComputeHash(Encoding.UTF8.GetBytes(input));

            /* Reformat to a string */
            foreach (byte x in hashByte)
                hash += String.Format("{0:x2}", x);
            
            return hash;
        }

        // Create a Hash which satisfies the difficulty level required for PoW
        public String Mine()
        {
            nonce = 0; // Initalise the nonce
            String hash = CreateHash(); // Hash the block

            String re = new string('0', difficulty); // A string for analysing the PoW requirement

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            while (!hash.StartsWith(re)) // Check the resultant hash against the "re" string
            {
                nonce++; // Increment the nonce should the difficulty level not be satisfied
                hash = CreateHash(); // Rehash with the new nonce as to generate a different hash
            }

            // Stop the stopwatch
            stopwatch.Stop();

            // Get the elapsed time
            totalTimeToMine = stopwatch.Elapsed;

            // Print the total elapsed time
            //Console.WriteLine($"Total time taken: {temestamp}");
           




            return hash; // Return the hash meeting the difficulty requirement
        }

        





        //    return hash; // Return the hash meeting the difficulty requirement
        //}

        // overloading creatHash function to take in a nonce, this is for the multithreading mine(). 
        public String CreateHash(long thNone)
        {
            String hash = String.Empty;
            SHA256 hasher = SHA256Managed.Create();

            String input = timestamp.ToString() + index + prevHash + thNone + merkleRoot;

            Byte[] hashByte = hasher.ComputeHash(Encoding.UTF8.GetBytes(input));

            foreach (byte x in hashByte)
                hash += String.Format("{0:x2}", x);

            return hash;
        }


        // Merkle Root Algorithm - Encodes transactions within a block into a single hash
        public static String MerkleRoot(List<Transaction> transactionList)
        {
            List<String> hashes = transactionList.Select(t => t.hash).ToList(); // Get a list of transaction hashes for "combining"
            
            // Handle Blocks with...
            if (hashes.Count == 0) // No transactions
            {
                return String.Empty;
            }
            if (hashes.Count == 1) // One transaction - hash with "self"
            {
                return HashCode.HashTools.combineHash(hashes[0], hashes[0]);
            }
            while (hashes.Count != 1) // Multiple transactions - Repeat until tree has been traversed
            {
                List<String> merkleLeaves = new List<String>(); // Keep track of current "level" of the tree

                for (int i=0; i<hashes.Count; i+=2) // Step over neighbouring pair combining each
                {
                    if (i == hashes.Count - 1)
                    {
                        merkleLeaves.Add(HashCode.HashTools.combineHash(hashes[i], hashes[i])); // Handle an odd number of leaves
                    }
                    else
                    {
                        merkleLeaves.Add(HashCode.HashTools.combineHash(hashes[i], hashes[i + 1])); // Hash neighbours leaves
                    }
                }
                hashes = merkleLeaves; // Update the working "layer"
            }
            return hashes[0]; // Return the root node
        }

        // Create reward for incentivising the mining of block
        public Transaction createRewardTransaction(List<Transaction> transactions)
        {
            double fees = transactions.Aggregate(0.0, (acc, t) => acc + t.fee); // Sum all transaction fees
            return new Transaction("Mine Rewards", minerAddress, (reward + fees), 0, ""); // Issue reward as a transaction in the new block
        }

        /* Concatenate all properties to output to the UI */
        public string ToString(string strategy = "default")


        {



            if (strategy == "greedy")
            {
                transactionList = transactionList.OrderByDescending(t => t.fee).ToList();

            }else if (strategy == "random")
            {
                // Use OrderBy with a random order to shuffle the transactionList
                Random random = new Random();
                transactionList = transactionList.OrderBy(t => random.Next()).ToList();
            }
            return "[BLOCK START]"
                + "\nIndex: " + index
                + "\tTimestamp: " + timestamp
                + "\nPrevious Hash: " + prevHash
                + "\n-- PoW --"
                + "\nDifficulty Level: " + difficulty 
                + "\nBlock Time: " + blockTime 
                + "\nNonce: " + nonce
                + "\nHash: " + hash
                + "\n-- Rewards --"
                + "\nReward: " + reward
                + "\nTime To Mine: " + totalTimeToMine.TotalMilliseconds + " ms"
                + "\nMiners Address: " + minerAddress
                + "\n-- " + transactionList.Count + " Transactions --"
                + "\nMerkle Root: " + merkleRoot
                + "\n" + String.Join("\n", transactionList)
                + "\n\n\n\n[BLOCK END]";
        }
    }
}
