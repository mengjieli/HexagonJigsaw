//Make By CSVCommand. Time 2018.5.11 15:21:32
using System;
using System.Collections.Generic;
using lib;

public class CoordConfig
{
	private List<string> list;
	public int id;
	public int x;
	public int y;

	public void Decode(List<string> list)
	{
		this.list = list;
		for(int i = 0; i < list.Count; i++)
		{
			if(i == 0)
			{
				id = (int)StringUtils.ToNumber(list[0]);
			}
			if(i == 1)
			{
				x = (int)StringUtils.ToNumber(list[1]);
			}
			if(i == 2)
			{
				y = (int)StringUtils.ToNumber(list[2]);
			}
		}
	}

	public void DecodeConfigItem()
	{
		for (int i = 0; i < list.Count; i++)
		{
		}
		list = null;
	}


	public static List<CoordConfig> Configs = new List<CoordConfig>();

	public static CoordConfig GetConfig(int key)
	{
		for(int i = 0; i < Configs.Count; i++)
		{
			if(Configs[i].id == key)
			{
				return Configs[i];
			}
		}
		return null;
	}

	public static void DecodeTable(string str)
	{
		Configs.Clear();
		str = StringUtils.Replace(str, '\r', '\n');
		str = StringUtils.Replace(str, "\n\n", '\n');
		List<List<string>> list = CSV.Parse(str);
		for(int i = 2,len = list.Count; i < len; i++)
		{
			CoordConfig item = new CoordConfig();
			item.Decode(list[i]);
			Configs.Add(item);
		}
	}

	public static CoordConfig GetConfigWidth(string paramName,object value)
	{
		Type t = typeof(CoordConfig);
		for (int i = 0; i < Configs.Count; i++)
		{
			object val = t.GetField(paramName).GetValue(Configs[i]);
			bool flag = false;
			if (val is string)
			{
				flag = ((string)value).Equals(val);
			}
			else
			{
				flag = val == value;
			}
			if (flag)
			{
				return Configs[i];
			}
		}
		return null;
	}

	public static void DecodeTableItem()
	{
		for(int i = 0,len = Configs.Count; i < len; i++)
		{
			Configs[i].DecodeConfigItem();
		}
	}

}