using System;
using System.Runtime.InteropServices;

namespace GedCom551
{
	public sealed class TGEDCOMUserReference : TGEDCOMTag
	{
		public string ReferenceType
		{
			get	{ return base.GetTagStringValue("TYPE"); }
			set	{ base.SetTagStringValue("TYPE", value); }
		}

		protected override void CreateObj(TGEDCOMObject AOwner, TGEDCOMObject AParent)
		{
			base.CreateObj(AOwner, AParent);
			this.FName = "REFN";
		}

		public TGEDCOMUserReference(TGEDCOMObject AOwner, TGEDCOMObject AParent, [In] string AName, [In] string AValue) : base(AOwner, AParent, AName, AValue)
		{
		}
	}
}
