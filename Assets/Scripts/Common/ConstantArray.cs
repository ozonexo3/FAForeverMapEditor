using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ozone
{
	public struct ArrayList<T>
	{
		T[] ConstantArray;

		int[] Ids;
		int _count;
		const int MaxMemoryAllocation = 2048;
		int _capacity;
		int MaxUsed;

		public void NewArray(int Capacity = MaxMemoryAllocation)
		{
			_capacity = Capacity;
			Ids = new int[Capacity];
			ConstantArray = new T[Capacity];
			_count = 0;
		}

		public int Count
		{
			get
			{
				return _count;
			}
		}

		public T Get(int index)
		{
			return ConstantArray[Ids[index]];
		}

		public void Add(T toAdd)
		{
			ConstantArray[_count] = toAdd;
			Ids[_count] = _count;
			_count++;
			MaxUsed = _count;
		}

		public void AddSafe(T toAdd)
		{
			for(int i = 0; i < MaxUsed; i++)
			{
				if(ConstantArray[i] == null)
				{
					ConstantArray[i] = toAdd;
					Ids[i] = _count;
					_count++;
					return;
				}
			}

			ConstantArray[MaxUsed] = toAdd;
			Ids[MaxUsed] = _count;
			_count++;
			MaxUsed++;
		}

		public void Remove(T toRemove)
		{

		}

		public void RemoveAt(int index)
		{

		}

		public void Sort()
		{
			int n = 0;
			int i = 0;
			for(i = 0; i < MaxUsed; i++)
			{
				if(ConstantArray[i] != null)
				{
					Ids[i] = n;
					n++;
				}
				else
				{
					Ids[i] = 0;
				}
			}
			MaxUsed = n;
		}
	}
}
