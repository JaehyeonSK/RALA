using System;
using System.IO;
using System.Threading;


namespace Test
{
	public class Book
	{
		private string Title;
		
		public void InputTitle()
		{
			Title = Console.ReadLine();
			Console.WriteLine(Title);
		}
	}

	public class Person 
	{
		public static void Main(string[] args)
		{
			int a = 3;
			int b = 5;
			int c = a + b;
			Console.WriteLine($"{a} + {b} = {c}");
			Book book = new Book();
			book.InputTitle();
		}
	}
	
	
}