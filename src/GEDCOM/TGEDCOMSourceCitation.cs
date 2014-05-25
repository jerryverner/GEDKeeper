using System;
using ExtUtils;

namespace GedCom551
{
	public sealed class TGEDCOMSourceCitation : TGEDCOMPointer
	{
		public bool IsPointer
		{
			get { return (!string.IsNullOrEmpty(base.XRef)); }
		}

		public StringList Description
		{
			get { return this.GetDescription(); }
			set { this.SetDescription(value); }
		}

		public string Page
		{
			get { return base.GetTagStringValue("PAGE"); }
			set { base.SetTagStringValue("PAGE", value); }
		}

		public int CertaintyAssessment
		{
			get { return base.GetTagIntegerValue("QUAY", 0); }
			set { base.SetTagIntegerValue("QUAY", value); }
		}

		private StringList GetDescription()
		{
			StringList description;

			if (!this.IsPointer)
			{
				description = base.GetTagStrings(this);
			}
			else
			{
				TGEDCOMRecord sourceRecord = base.Value;
				if (sourceRecord != null && sourceRecord is TGEDCOMSourceRecord) {
					description = ((sourceRecord as TGEDCOMSourceRecord).Title);
				} else {
					description = new StringList();
				}
			}

			return description;
		}

		private void SetDescription(StringList value)
		{
			this.Clear();
			base.SetTagStrings(this, value);
		}

		protected override void CreateObj(TGEDCOMTree owner, GEDCOMObject parent)
		{
			base.CreateObj(owner, parent);
			this.fName = "SOUR";
		}

		protected override string GetStringValue()
		{
			string result = this.IsPointer ? base.GetStringValue() : this.fStringValue;
			return result;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
			}
			base.Dispose(disposing);
		}

		public override void Clear()
		{
			base.Clear();
		}

		public override bool IsEmpty()
		{
			bool result;
			if (this.IsPointer) {
				result = base.IsEmpty();
			} else {
				result = (this.fStringValue == "" && base.Count == 0);
			}
			return result;
		}

		public override string ParseString(string strValue)
		{
			this.fStringValue = "";
			base.XRef = "";
			string result = strValue;
			if (!string.IsNullOrEmpty(result))
			{
				result = GEDCOMUtils.ExtractDelimiter(result, 0);
				result = base.ParseString(result);
				if (!this.IsPointer)
				{
					this.fStringValue = result;
					result = "";
				}
			}
			return result;
		}

		public TGEDCOMSourceCitation(TGEDCOMTree owner, GEDCOMObject parent, string tagName, string tagValue) : base(owner, parent, tagName, tagValue)
		{
		}
	}
}
