using System.Data.Entity;

namespace Server
{
    public static class DbManager
    {
        //Класс отображающий контекст базы данных приложения.
        public class PerformanceDbContext : DbContext
        {
            /// <summary>
            /// Конструктор контекста.
            /// </summary>
            public PerformanceDbContext() : base("Performance") { }
            /// <summary>
            /// Свойство отражающее таблицу в базе данных.
            /// </summary>
            public DbSet<Performance> Performances { get; set; }
        }

        //Экземпляр класса контекста базы данных приложения.
        private static PerformanceDbContext _instance;

        /// <summary>
        /// Свойство возвращает экземпляр контекста базы данных приложения. Если экземпляр ещё не был создан, то он создаётся.
        /// </summary>
        public static PerformanceDbContext Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new PerformanceDbContext();
                return _instance;
            }
        }
    }
}