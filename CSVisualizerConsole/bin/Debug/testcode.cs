using System;
using System.IO;
using System.Threading;


namespace Test
{
	class Person 
	{
		public static void Main(string[] args)
		{
			int a = 3;
			int b = 5;
			int c = a + b;
			Console.WriteLine($"{a} + {b} = {c}");
		}
	}
	
	class Book
	{
		private string Title;
		
		public void InputTitle()
		{
			Title = Console.ReadLine();
		}
	}

	Person.Main(null);
}