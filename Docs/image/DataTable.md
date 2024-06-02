# DataTable

该工具可将Excel数据生成代码,例如,如下的Excel表将生成这样的代码:

![1717241650278](image/DataTable/1717241650278.png)

```C#
using UnityEngine;
	public class _YourDTClassName
	{
		public static _TestDataBook TestData => instance._TestData;

		private static _YourDTClassName instance = new();
		private _TestDataBook _TestData = new();

		public class _TestDataBook
		{
			public readonly Sheet4Entry[] Sheet4 =
			{
				new(21,1,new Vector3(1f,2f,3f)),
				new(22,2,new Vector3(3f,4f,5f)),
				new(23,3,new Vector3(-2f,-1f,0f)),
			};
			/// <param name="Atk">攻击力</param>
			/// <param name="Def">防御力</param>
			/// <param name="Position">位置</param>
			public record Sheet4Entry
			(
				int Atk,
				int Def,
				Vector3 Position
			);
		}
	}
```

可以直接通过表格里的字段名访问数据,并且不需要反序列化等操作

## Excel数据语法

1. 首行第一格写#CSV或#KVP,前者表示表格,后者表示都是键值对(类似Dictionary)
2. 在第二三行的第一列写#变量和#注释,两者顺序可互换,前者是代码里的标识符,后者是注释
3.
