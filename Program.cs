using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


class Program
{

	static char Menu()
	{
		Console.Clear();
		Console.WriteLine("A - wplac pieniadze na konto");
		Console.WriteLine("O - odejmij pieniadze z konta");
		Console.WriteLine("T - transferuj pieniadze");
		Console.WriteLine("S - sprawdz stan swojego konta");
		Console.WriteLine("D - sprawdz stan konta (Anonymous)");
		Console.WriteLine("H - sprawdz historie przelewow");
		Console.WriteLine("K - koniec programu\n");
		return Console.ReadKey(true).KeyChar;
	}

	static void Main(string[] args)
	{
		uint pln;
		ushort cents;
		Console.Write("Podaj imie: ");
		string name = Console.ReadLine();
		Console.Write("\nPodaj kwote w zl: ");
		uint zl = Convert.ToUInt32(Console.ReadLine());
		Console.Write("\nPodaj ilosc groszy: ");
		ushort gr = Convert.ToUInt16(Console.ReadLine());
		BankAccount a = new BankAccount(name, zl, gr);
		Console.Clear();
		Console.Write("Podaj imie dla drugiego konta: ");
		string name2 = Console.ReadLine();
		if(name2 == "")
		{
			name2 = "Anonymous";
		}
		Console.Write("\nPodaj kwote w zl: ");
		uint zl2;
		string tmpZl = Console.ReadLine();
		if(tmpZl == "")
		{
			zl2 = 0;
		}
		else
		{
			zl2 = Convert.ToUInt32(Console.ReadLine());
		}
		Console.Write("\nPodaj ilosc groszy: ");
		ushort gr2;
		string tmpGr = Console.ReadLine();
		if(tmpGr == "")
		{
			gr2 = 0;
		}
		else
		{
			gr2 = Convert.ToUInt16(Console.ReadLine());
		}
		BankAccount b = new BankAccount(name2, zl2, gr2);
		List<string> historyOfTransfers = new List<string>();
		char c;
		pln = 0;
		cents = 0;
		
		do
		{
			c = Menu();
			switch(c)
			{
				case 'a':
				case 'A':
					Console.Write("Podaj ilosc Zl: ");
					pln = Convert.ToUInt32(Console.ReadLine());
					Console.Write("\nPodaj ilosc Groszy: ");
					cents = Convert.ToUInt16(Console.ReadLine());
					a.Deposit(pln, cents);
					break;

				case 'o':
				case 'O':
					Console.Write("Podaj ilosc Zl: ");
					pln = Convert.ToUInt32(Console.ReadLine());
					Console.Write("\nPodaj ilosc Groszy: ");
					cents = Convert.ToUInt16(Console.ReadLine());
					a.Payment(pln, cents);
					break;

				case 't':
				case 'T':
					Console.Write("Podaj ilosc Zl: ");
					pln = Convert.ToUInt32(Console.ReadLine());
					Console.Write("\nPodaj ilosc Groszy: ");
					cents = Convert.ToUInt16(Console.ReadLine());
					bool checkTransfer = a.Transfer(b, pln, cents);
					if(checkTransfer)
					{
						TimeSpan timeNow = DateTime.Now.TimeOfDay;
						TimeSpan trimmedTimeNow = new TimeSpan(timeNow.Hours, timeNow.Minutes, timeNow.Seconds);
						string history = "Z konta (" + a.Name + "); Wyslano: " + pln + ", " + cents + "; do: " + b.Name + "; o godzinie: " + trimmedTimeNow;
						historyOfTransfers.Add(history);
					}
					break;

				case 's':
				case 'S':
					a.CheckSaldo();
					break;

				case 'd':
				case 'D':
					b.CheckSaldo();
					break;

				case 'h':
				case 'H':
					for (int i = 0; i < historyOfTransfers.Count; i++ )
					{
						Console.WriteLine("[{0}]: {1}", i+1, historyOfTransfers[i]);
					}
					break;
			}
			Console.ReadKey();
		}
		while(!(c == 'k' || c == 'K'));

		Console.ReadKey();
	}

}

interface IAccount
{
	uint Pln { get; }
	ushort Cents { get; }
	string Name { get; }

	void Deposit(uint pln, ushort cents);
	bool Payment(uint pln, ushort cents);
	bool Transfer(IAccount destination, uint pln, ushort cents);
	void CheckSaldo();
}

class BankAccount : IAccount
{
	public uint Pln { get; private set; }
	private ushort cents;
	public ushort Cents { 
		get
		{
			return cents;
		}
		private set
		{
			if(value < 0) // jeśli groszy jest poniżej zera to ustaw domyślnie na 0
			{
				cents = 0;
			}
			else if(value > 100) // jeżeli groszy jest więcej niż 100 to zamień na złotówki i grosze
			{
				uint tmpPln = (uint)(cents / 100);
				if (tmpPln > 0)
					this.Pln += tmpPln;
				cents = (ushort)(cents - (tmpPln * 100));
			}
			else
			{
				cents = value;
			}
		}
	}
	public string Name { get; private set; }

	public BankAccount(string name, uint pln, ushort cents)
	{
		this.Name = name;
		this.Pln = pln;
		this.Cents = cents;
	}

	public void Deposit(uint pln, ushort cents) // wpłata
	{
		uint amountAccount = (this.Pln * 100) + this.Cents;
		uint amount = (pln * 100) + cents;
		uint resultAmount = amountAccount + amount;
		uint tmpPln = resultAmount / 100;
		if (tmpPln >= 0)
			this.Pln = tmpPln;
		ushort tmpCents = (ushort)(resultAmount - (tmpPln * 100));
		this.Pln = tmpPln;
		this.Cents = tmpCents;
	}

	public bool Payment(uint pln, ushort cents) // wypłata
	{
		uint amountAccount = (this.Pln * 100) + this.Cents;
		uint amount = (pln * 100) + cents;
		if (amountAccount >= amount) // sprawdź czy saldo jest większe od kwoty do wypłaty
		{
			uint resultAmount = (amountAccount - amount);
			uint tmpPln = resultAmount / 100;
			if (tmpPln >= 0)
				this.Pln = tmpPln;
			ushort tmpCents = (ushort)(resultAmount - (tmpPln * 100));
			this.Cents = tmpCents;
			return true;
		}

		Console.WriteLine("Za malo srodkow na koncie");
		return false;
	}

	public bool Transfer(IAccount destination, uint pln, ushort cents) // transfer na inne konto bankowe
	{
		bool checkPayment = Payment(pln, cents);
		if (checkPayment)
			destination.Deposit(pln, cents);

		return checkPayment;
	}

	public void CheckSaldo()
	{
		Console.WriteLine("--- Stan konta (" + this.Name + ") ---\nZl: " + this.Pln + ", Groszy: " + this.Cents);
	}
}