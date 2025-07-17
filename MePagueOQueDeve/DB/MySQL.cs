using MePagueOQueDeve.Class;
using MySqlConnector;
using System.Data;

namespace MePagueOQueDeve.DB
{
	public class MySQL
	{
		MySqlConnection _connection;
		public string ConnectionString;

		public MySQL()
		{
			ConnectionString = @$"Server={GlobalSettings.MySQLServer};User ID={GlobalSettings.MySQLUser};
								Password={GlobalSettings.MySQLPassword};Database={GlobalSettings.MySQLSchema}";
			_connection = new MySqlConnection(ConnectionString);
			_connection.Open();
		}

		#region Read Values
		public int ReadInt(object Value)
		{
			int value = 0;

			try
			{
				if (Value != DBNull.Value)
					value = Convert.ToInt32(Value);
			}
			catch { }

			return value;
		}

		public string ReadString(object Value)
		{
			string value = string.Empty;

			try
			{
				if (Value != DBNull.Value)
					value = Convert.ToString(Value);
			}
			catch { }

			return value;
		}

		public double ReadDouble(object Value)
		{
			double value = 0d;
			try
			{
				if (Value != DBNull.Value)
					value = Convert.ToDouble(Value);
			}
			catch { }
			return value;
		}

		public bool ReadBool(object Value)
		{
			bool value = false;

			try
			{
				if (Value != DBNull.Value)
					value = Convert.ToBoolean(Value);
			}
			catch { }

			return value;
		}

		public DateTime ReadDateTime(object Value)
		{
			DateTime value = DateTime.MinValue;

			try
			{
				if (Value != DBNull.Value)
					value = Convert.ToDateTime(Value);
			}
			catch { }

			return value;
		}

		public TimeSpan ReadTimeSpan(object Value)
		{
			TimeSpan value = TimeSpan.Zero;
			try
			{
				if (Value != DBNull.Value)
					TimeSpan.TryParse(Value.ToString(), out value);
			}catch { }
			return value;
		}
		#endregion

		#region Help Functions
		public void ExecuteCommand(string Command)
		{
			using (MySqlConnection connection = new MySqlConnection(ConnectionString))
			{
				connection.Open();

				MySqlCommand command = new MySqlCommand(Command, connection);

				command.ExecuteNonQuery();
			}
		}

		public MySqlDataReader ReadData(string Command)
		{
			if (_connection.State == ConnectionState.Closed)
			{
				_connection.Open();
			}

			MySqlCommand command = new MySqlCommand(Command, _connection);

			MySqlDataReader reader = command.ExecuteReader();

			return reader;
		}

		public ConnectionState State()
		{
			return _connection.State;
		}
		public void Close()
		{
			if (_connection.State == ConnectionState.Open)
			{
				_connection.Close();
			}
		}
		#endregion
	}
}
