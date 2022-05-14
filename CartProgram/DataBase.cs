namespace CartProgram;

public interface IDataBase {
	void Insert(User user, int pId, int amount);
	void Delete(User user, int pId, int amount);
	int SearchCount(User user, int pId);
	(string, string, string, int, int) GetMetadata(int pId);
	object GetCouponInfo(int pId);
}

public class SimpleDB : IDataBase {
	public const int TISSUE_ID = 1;
	public const int APPLE_ID = 2;
	public const int MANGO_ID = 3;
	public const int COUPON_75_OFF_ID = 4;
	public const int COUPON_100_ID = 5;

	public const int PRODUCT_PRIORITY = -1;

	public static readonly int[] InventoryProducts = new int[] { TISSUE_ID, APPLE_ID, MANGO_ID, };
	public static readonly int[] UserCoupons = new int[] { COUPON_75_OFF_ID, COUPON_100_ID };
	
	private List<(User, int, int)> records = new();
	private Dictionary<int, (string, string, string, int, int)> product_table = new() {
		[TISSUE_ID] = ("柔芙輕巧包抽取式衛生紙", "120抽x20包x4袋", "【箱購】", 580, PRODUCT_PRIORITY),
		[APPLE_ID] = ("智利富士蘋果", "135g", "", 14, PRODUCT_PRIORITY),
		[MANGO_ID] = ("頂級愛文芒果禮盒 ", "(約1.6kg/盒)", "【預購】", 468, PRODUCT_PRIORITY),
		[COUPON_75_OFF_ID] = ("消費75折", "不限金額", "【折價券】", 0, 1),
		[COUPON_100_ID] = ("折價100元", "不限金額", "【折價券】", 0, 2),
	};
	private Dictionary<int, object> coupon_info = new() {
		[COUPON_75_OFF_ID] = 75,
		[COUPON_100_ID] = 100,
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

	public (string, string, string, int, int) GetMetadata(int pId) {
		return product_table[pId];
	}

	public object GetCouponInfo(int pId) {
		return coupon_info[pId];
	}
}