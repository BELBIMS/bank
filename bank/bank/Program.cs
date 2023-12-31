using System;
using System.Threading;

class Account
{
    private decimal balance;
    private readonly object lockObject = new object(); // Объект для блокировки операций чтения/записи баланса

    public decimal Balance
    {
        get
        {
            lock (lockObject)
            {
                return balance; // Возвращает текущий баланс счета, используя блокировку операции чтения
            }
        }
    }

    public void Deposit(decimal amount)
    {
        lock (lockObject)
        {
            balance += amount;
            Console.WriteLine($"Баланс пополнен на {amount}. Текущий баланс: {balance}"); // Пополняет баланс на указанную сумму и выводит информацию о новом балансе
        }
    }

    public void Withdraw(decimal amount)
    {
        lock (lockObject)
        {
            if (balance >= amount)
            {
                balance -= amount;
                Console.WriteLine($"Со счета снято {amount}. Текущий баланс: {balance}"); // Снимает указанную сумму со счета и выводит информацию о новом балансе
            }
            else
            {
                Console.WriteLine("Недостаточно средств на счете."); // Выводит сообщение об отсутствии достаточных средств на счете
            }
        }
    }

    public bool WaitForBalance(decimal targetBalance, int timeout)
    {
        DateTime startTime = DateTime.Now;
        while (Balance < targetBalance)
        {
            if (DateTime.Now - startTime >= TimeSpan.FromMilliseconds(timeout))
                return false; // Если время ожидания истекло, возвращает false

            Thread.Sleep(100); // Пауза между проверками
        }
        return true; // Если требуемый баланс достигнут, возвращает true
    }
}

class BankApp
{
    static void Main()
    {
        Account account = new Account();

        // Запуск потока для многократного пополнения счета
        Thread depositThread = new Thread(() =>
        {
            Random random = new Random();
            for (int i = 0; i < 10; i++)
            {
                decimal depositAmount = random.Next(100, 1000);
                account.Deposit(depositAmount);
                Thread.Sleep(500); // Пауза между пополнениями
            }
        });
        depositThread.Start();

        // Ожидание пополнения баланса до требуемой суммы, а затем снятие денег
        decimal targetWithdrawalAmount = 5000;
        if (account.WaitForBalance(targetWithdrawalAmount, 5000))
        {
            account.Withdraw(targetWithdrawalAmount);
        }
        else
        {
            Console.WriteLine($"Не удалось накопить требуемую сумму {targetWithdrawalAmount}."); // Выводит сообщение о неудаче при накоплении требуемой суммы
        }

        Console.WriteLine($"Остаток на балансе: {account.Balance}"); // Выводит текущий остаток на балансе
    }
}
