using System.Xml.Serialization;
// ReSharper disable once CheckNamespace
namespace Nez
{
	// ---- AngelCode BmFont XML serializer ----------------------
	// ---- By DeadlyDan @ deadlydan@gmail.com -------------------
	// ---- There's no license restrictions, use as you will. ----
	// ---- Credits to http://www.angelcode.com/ -----------------
	public class BitmapFontKerning
	{
		[XmlAttribute( "first" )]
		public int first;
		
		[XmlAttribute( "second" )]
		public int second;
		
		[XmlAttribute( "amount" )]
		public int amount;
	}
}
