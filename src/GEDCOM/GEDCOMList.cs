using System;
using System.Collections.Generic;
using System.IO;

namespace GedCom551
{
	public interface IGEDCOMListEnumerator
	{
		object Current
		{
			get;
		}

		GEDCOMObject Owner
		{
			get;
		}

		bool MoveNext();
		void Reset();
	}

	public sealed class GEDCOMList<T> : IDisposable
	{
		#region ListEnumerator

		public sealed class GEDCOMListEnumerator : IGEDCOMListEnumerator
		{
			private readonly GEDCOMList<T> fList;
			private int fIndex;

			public GEDCOMListEnumerator(GEDCOMList<T> list)
			{
				this.fList = list;
				this.fIndex = -1;
			}

			void IGEDCOMListEnumerator.Reset()
			{
				this.fIndex = -1;
			}

			GEDCOMObject IGEDCOMListEnumerator.Owner
			{
				get {
					return this.fList.fOwner;
				}
			}

			bool IGEDCOMListEnumerator.MoveNext()
			{
				this.fIndex++;
				return (this.fIndex < this.fList.Count);
			}

			object IGEDCOMListEnumerator.Current
			{
				get {
					try
					{
						return this.fList[fIndex];
					}
					catch (IndexOutOfRangeException)
					{
						throw new InvalidOperationException();
					}
				}
			}
		}

		#endregion

		
		private List<T> fList = null; // lazy implementation
		private readonly GEDCOMObject fOwner;
		private bool fDisposed;

		public int Count
		{
			get {
				return ((this.fList == null) ? 0 : this.fList.Count);
			}
		}

		public T this[int index]
		{
			get {
				return ((this.fList == null) ? default(T) : this.fList[index]);
			}
		}

		public GEDCOMList(GEDCOMObject owner)
		{
			this.fOwner = owner;
		}

		public void Dispose()
		{
			if (!this.fDisposed)
			{
				this.Clear();
				//this.fList.Free(); isnot IDisposable
				this.fDisposed = true;
			}
		}

		public IGEDCOMListEnumerator GetEnumerator()
		{
			return new GEDCOMListEnumerator(this);
		}

		public T Add(T item)
		{
			if (item != null)
			{
				if (this.fList == null)
				{
					this.fList = new List<T>();
				}

				this.fList.Add(item);
			}

			return item;
		}

		public void Clear()
		{
			if (this.fList != null)
			{
				for (int I = this.fList.Count - 1; I >= 0; I--)
				{
					(this.fList[I] as GEDCOMObject).Dispose();
				}
				this.fList.Clear();
			}
		}

		public void Delete(int index)
		{
			if (this.fList != null)
			{
                (this.fList[index] as GEDCOMObject).Dispose();
				this.fList.RemoveAt(index);
			}
		}

		public void DeleteObject(T item)
		{
			if (this.fList != null)
			{
				int idx = this.fList.IndexOf(item);
				if (idx >= 0)
				{
					this.Delete(idx);
				}
			}
		}

		public void Exchange(int index1, int index2)
		{
			if (this.fList != null)
			{
				if (index1 >= 0 && index1 < this.fList.Count && index2 >= 0 && index2 < this.fList.Count)
				{
					T tmp = this.fList[index1];
					this.fList[index1] = this.fList[index2];
					this.fList[index2] = tmp;
				}
			}
		}

		public T Extract(int index)
		{
			if (this.fList != null) {
				T result = this.fList[index];
				this.fList.RemoveAt(index);
				return result;
			} else {
				return default(T);
			}
		}

		public int IndexOfObject(T item)
		{
			return (this.fList == null) ? -1 : this.fList.IndexOf(item);
		}

		public void SaveToStream(StreamWriter stream)
		{
			if (this.fList != null)
			{
				int num = this.fList.Count - 1;
				for (int I = 0; I <= num; I++)
				{
                    if (this.fList[I] is TGEDCOMTag)
					{
                        (this.fList[I] as TGEDCOMTag).SaveToStream(stream);
					}
				}
			}
		}

		public void ReplaceXRefs(XRefReplacer map)
		{
			if (this.fList != null)
			{
				int num = this.fList.Count - 1;
				for (int i = 0; i <= num; i++)
				{
                    if (this.fList[i] is TGEDCOMTag)
					{
                        (this.fList[i] as TGEDCOMTag).ReplaceXRefs(map);
					}
				}
			}
		}

		public void ResetOwner(TGEDCOMTree newOwner)
		{
			if (this.fList != null)
			{
				int num = this.fList.Count - 1;
				for (int i = 0; i <= num; i++)
				{
                    (this.fList[i] as TGEDCOMTag).ResetOwner(newOwner);
				}
			}
			//this._owner = newOwner;
		}

		public void Pack()
		{
			if (this.fList != null)
			{
				for (int i = this.fList.Count - 1; i >= 0; i--)
				{
                    if (this.fList[i] is TGEDCOMTag)
					{
                        TGEDCOMTag tag = this.fList[i] as TGEDCOMTag;
						tag.Pack();
						if (tag.IsEmpty() && tag.IsEmptySkip())
						{
							this.Delete(i);
						}
					}
				}
			}
		}
	}
}
