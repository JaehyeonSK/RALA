using System;
using System.IO;
using System.Threading;


namespace Test
{
	public class Node
	{
		public int value;
		public Node next;
	}

	public class Program
	{
		public static void Main(string[] args)
		{
			Node n1 = new Node();
			Node n2 = new Node();

			n1.value = 1;
			n1.next = n2;
		
			n2.value = 2;
			n2.next = null;			
		}
	}
	
	
}