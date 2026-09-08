using System.Collections.Generic;

namespace com.swecial.util {
    public static class Locker {

        private static Dictionary<string, Lock> locks;

        public static Lock getLock(string lockId) {
            if (locks == null) {
                locks = new Dictionary<string, Lock>();
            }
            Lock myLock = null;
            lock (locks) {
                if (!locks.TryGetValue(lockId, out myLock)) {
                    myLock = new Lock();
                    myLock.lockId = lockId;
                    locks.Add(lockId, myLock);
                    return myLock;
                } else {
                    return myLock;
                }
            }
        }
        public static void deleteLock(Lock myLock) {
            lock (locks) {
                locks.Remove(myLock.lockId);
            }
        }
    }
    public class Lock {
        public string lockId;
    }
}