using System.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HardwareInterface
{
    public class SyncList<T>
    {
        private List<T> mLst = new List<T>();
        private ReaderWriterLockSlim mLock = new ReaderWriterLockSlim();

        //*********************************
        //
        //*********************************
        public void Add(T newItem)
        {
            mLock.EnterWriteLock();
            try
            {
                mLst.Add(newItem);
            }
            finally
            {
                mLock.ExitWriteLock();
            }
        }
        //*********************************
        //
        //*********************************
        public T TakeFirst()
        {
            mLock.EnterReadLock();
            try
            {
                T tmp = mLst[0];
                mLst.RemoveAt(0);

                return tmp;
            }
            finally
            {
                mLock.ExitReadLock();
            }
        }
        //*********************************
        //
        //*********************************
        public T TakeAt(int index)
        {
            mLock.EnterReadLock();
            try
            {
                T tmp = mLst[index];
                mLst.RemoveAt(index);

                return tmp;
            }
            finally
            {
                mLock.ExitReadLock();
            }
        }
        //*********************************
        //
        //*********************************
        public T GetAt(int index)
        {
            mLock.EnterReadLock();
            try
            {
                return mLst[index];
            }
            finally
            {
                mLock.ExitReadLock();
            }
        }
        //*********************************
        //
        //*********************************
        public void RemoveAt(int index)
        {
            mLock.EnterWriteLock();
            try
            {
                mLst.RemoveAt(index);
            }
            finally
            {
                mLock.ExitWriteLock();
            }
        }
        //*********************************
        //
        //*********************************
        public int IndexOf(T itemToSearch)
        {
            mLock.EnterReadLock();
            try
            {
                return mLst.IndexOf(itemToSearch);
            }
            finally
            {
                mLock.ExitReadLock();
            }
        }
        //*********************************
        //
        //*********************************
        public int Count
        {
            get 
            {
                mLock.EnterReadLock();
                try
                {
                    return mLst.Count;
                }
                finally
                {
                    mLock.ExitReadLock();
                }
            }
        }
        //*********************************
        //
        //*********************************
        public void Clear()
        {
            mLock.EnterReadLock();
            try
            {
                mLst.Clear();
            }
            finally
            {
                mLock.ExitReadLock();
            } 
        }
    }
}