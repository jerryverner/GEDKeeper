using System;
using System.IO;
using System.Runtime.InteropServices;

using GKCore.Sys;

namespace GedCom551
{
	public sealed class TGEDCOMMultimediaLink : TGEDCOMPointer
	{
		private TGEDCOMListEx<TGEDCOMFileReference> _FileReferences;

		public TGEDCOMListEx<TGEDCOMFileReference> FileReferences
		{
			get { return this._FileReferences; }
		}

		public bool IsPointer
		{
			get { return (!string.IsNullOrEmpty(base.XRef)); }
		}

		public string Title
		{
			get { return base.GetTagStringValue("TITL"); }
			set { base.SetTagStringValue("TITL", value); }
		}

		public bool IsPrimary
		{
			get { return this.GetIsPrimary(); }
			set { this.SetIsPrimary(value); }
		}

		private bool GetIsPrimary()
		{
			TGEDCOMTag tag = base.FindTag("_PRIM", 0);
			return tag != null && (tag.StringValue == "Y");
		}

		private void SetIsPrimary([In] bool Value)
		{
			if (Value)
			{
				TGEDCOMTag tag = base.FindTag("_PRIM", 0);
				if (tag == null)
				{
					tag = this.AddTag("_PRIM", "", null);
				}
				tag.StringValue = "Y";
			}
			else
			{
				base.DeleteTag("_PRIM");
			}
		}

		protected override void CreateObj(TGEDCOMObject AOwner, TGEDCOMObject AParent)
		{
			base.CreateObj(AOwner, AParent);
			this.FName = "OBJE";
			this._FileReferences = new TGEDCOMListEx<TGEDCOMFileReference>(this);
		}

		protected override string GetStringValue()
		{
			string Result;
			if (this.IsPointer)
			{
				Result = base.GetStringValue();
			}
			else
			{
				Result = this.FStringValue;
			}
			return Result;
		}

		public override void Dispose()
		{
			if (!this.Disposed_)
			{
				this._FileReferences.Dispose();

				base.Dispose();
				this.Disposed_ = true;
			}
		}

		public override TGEDCOMTag AddTag([In] string ATag, [In] string AValue, Type AClass)
		{
			TGEDCOMTag Result;
			if (ATag == "FILE")
			{
				Result = this.FileReferences.Add(new TGEDCOMFileReference(base.Owner, this, ATag, AValue));
			}
			else
			{
				Result = base.AddTag(ATag, AValue, AClass);
			}
			return Result;
		}

		public override void Clear()
		{
			base.Clear();
			this._FileReferences.Clear();
		}

		public override bool IsEmpty()
		{
			bool Result;
			if (this.IsPointer)
			{
				Result = base.IsEmpty();
			}
			else
			{
				Result = (base.Count == 0 && (this._FileReferences.Count == 0));
			}
			return Result;
		}

		public override string ParseString([In] string AString)
		{
			this.FStringValue = "";
			return base.ParseString(AString);
		}

		public override void ResetOwner(TGEDCOMObject AOwner)
		{
			base.ResetOwner(AOwner);
			this._FileReferences.ResetOwner(AOwner);
		}

		public override void SaveToStream(StreamWriter AStream)
		{
			base.SaveToStream(AStream);
			this._FileReferences.SaveToStream(AStream);
		}

		public TGEDCOMMultimediaLink(TGEDCOMObject AOwner, TGEDCOMObject AParent, [In] string AName, [In] string AValue) : base(AOwner, AParent, AName, AValue)
		{
		}
	}
}
