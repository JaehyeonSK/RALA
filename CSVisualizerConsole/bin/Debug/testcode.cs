using System;

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
	
	Person.Main(null);
}