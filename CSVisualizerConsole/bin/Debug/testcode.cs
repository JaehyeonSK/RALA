using System;
using System.IO;
using System.Threading;


namespace Test
{
	public class Book
	{
		private string Title;
        public int a;
		
		public void InputTitle(string title)
		{	
			Title = title;
		}
	}

	public class Program
	{
		public static void Main(string[] args)
		{
			int a = 3;
			int b = 5;
			int c;
            			int d;
			c = a;
			d = 3;
			Console.WriteLine($"{a} + {b} = {c}");
			Book book = new Book();
			book.InputTitle("Hello");
		}
	}
	
	
}