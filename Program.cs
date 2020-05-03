using System;  
using System.Threading;
using System.Threading.Tasks;  
using System.Collections.Generic;
class ThreadPoolSample  
{       
    // Parameters
    static readonly int EXECUTIONS = 100;
    static readonly int MAX_ID = 10;
    static readonly int MAX_WORK_SECONDS = 500;
    static List<Entity> entityPersistence = new List<Entity>();
    static void Main(string[] args)  
    {  
        GeneratePersistence();
        List<Task> taskPool = new List<Task>();

        Console.WriteLine($"------------ Starting Execution ---------------");
        
        for(int i = 0; i < EXECUTIONS; i++){
            taskPool.Add(Task.Run(() => {
                                            Random rndGen = new Random();
                                            int id1 = rndGen.Next(MAX_ID + 1);
                                            int id2 = rndGen.Next(MAX_ID + 1);
                                            Entity syncObject1 = entityPersistence.Find(e => e.id == id1);
                                            Entity syncObject2 = entityPersistence.Find(e => e.id == id2);
                                            new ExecutionObject(syncObject1,syncObject2).Execute(MAX_WORK_SECONDS);
                                        }));
        }
        Task.WaitAll(taskPool.ToArray());
        Console.WriteLine("------------ Finished Execution ---------------");
    }

    static void GeneratePersistence(){
        Console.Write("Generated Persistence: ");
        for(int i = 0; i <= MAX_ID; i++){
            entityPersistence.Add(new Entity(i));
        }            
        entityPersistence.ForEach(i => Console.Write("{0}\t", i));
        Console.WriteLine();
    }
}  

class ExecutionObject {
    internal Entity syncObject1, syncObject2;
    public ExecutionObject(Entity creditor, Entity customer) {
        this.syncObject1 = creditor;
        this.syncObject2 = customer;
    }
    public void Execute(int maxWorkSeconds){
        Random rnd = new Random();
        Entity comparedFirst;
        Entity comparedSecond;
        
        // IMPORTANT: To avoid Deadlock, must stablish same lock order.
        if(syncObject1.CompareTo(syncObject2) >= 1){
            comparedFirst = syncObject1;
            comparedSecond = syncObject2;
        } else {
            comparedFirst = syncObject2;
            comparedSecond = syncObject1;
        }

        lock(comparedFirst) lock(comparedSecond){
            TrackLock(syncObject1); TrackLock(syncObject2);
            Thread.Sleep(rnd.Next(maxWorkSeconds)); // Doing Some Work
            TrackUnlock(syncObject1); TrackUnlock(syncObject2);
       }
    }
    private void TrackUnlock(Entity entity){
        Console.WriteLine($"{DateTime.Now.ToString("hh:mm:ss.fff tt")} | {Thread.CurrentThread.ManagedThreadId} | {syncObject1.id} - {syncObject2.id}|: Unblocked Entity: {entity.id}");
    }
    private void TrackLock(Entity entity){
        Console.WriteLine($"{DateTime.Now.ToString("hh:mm:ss.fff tt")} | {Thread.CurrentThread.ManagedThreadId} | {syncObject1.id} - {syncObject2.id}|: Locked Entity: {entity.id}");
    }
}

class Entity : IComparable{
    readonly internal int id;
    public Entity(int id) => this.id = id;

    public override string ToString() =>  id.ToString();
    public int CompareTo(object obj){
        if(obj == null) return 1;
        Entity comparated = obj as Entity;
        if(comparated != null){
            return this.id.CompareTo(comparated.id);
        } else return 0;
    }
}