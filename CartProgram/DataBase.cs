namespace CartProgram;

public interface IDataBase {
	void Insert(User user, int pId, int amount);
	void Delete(User user, int pId, int amount);
	int SearchCount(User user, int pId);
	(string, string, string) GetMetadata(int pId);
}

public class SimpleDB : IDataBase {
	public const int TISSUE_ID = 0;
	
	List<(User, int, int)> records = new();
	Dictionary<int, (string, string, string)> product_table = new() {
		[TISSUE_ID] = ("柔芙輕巧包抽取式衛生紙", "120抽x20包x4袋", "【箱購】"),
	};

	public void Insert(User user, int pId, int amount) {
		records.Add((user, pId, amount));
	}

	public void Delete(User user, int pId, int amount) {
		var remaining = records.Where(r => r.Item1 == user && r.Item2 == pId).Select(r => r.Item3).Sum();
		if (remaining - amount < 0)
			throw new InvalidOperationException();
		records.Add((user, pId, -amount));
	}

	public int SearchCount(User user, int pId) {
		return records.Where(r => r.Item1 == user && r.Item2 == pId).Select(r => r.Item3).Sum();
	}

	public (string, string, string) GetMetadata(int pId) {
		return product_table[pId];
	}
}