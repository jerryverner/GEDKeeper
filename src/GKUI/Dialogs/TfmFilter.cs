﻿using System;
using System.Windows.Forms;

using GedCom551;
using GKCore;
using GKCore.Sys;
using GKUI.Controls;
using GKUI.Lists;

namespace GKUI
{
	public partial class TfmFilter : Form
	{
		private TfmBase FBase;

		public TfmBase Base
		{
			get { return this.FBase; }
		}

		private object[] StringsToArray(TStrings aStrings)
		{
			object[] result = new object[aStrings.Count];
			for (int i = 0; i <= aStrings.Count - 1; i++) {
				result[i] = aStrings[i];
			}
			return result;
		}

		private void rgLifeClick(object sender, EventArgs e)
		{
			this.edAliveBeforeDate.Enabled = this.RadioButton4.Checked;
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			this.Base.Filter.Clear();
			this.Base.ApplyFilter();
		}

		private void btnAccept_Click(object sender, EventArgs e)
		{
			try
			{
				this.AcceptChanges();
				base.DialogResult = DialogResult.OK;
			}
			catch (Exception E)
			{
				SysUtils.LogWrite("TfmFilter.Accept(): " + E.Message);
				base.DialogResult = DialogResult.None;
			}
		}

		private void AcceptChanges()
		{
			string fs = this.edName.Text.Trim();
			if (fs != "" && fs != "*" && GKUI.TfmGEDKeeper.Instance.Options.NameFilters.IndexOf(fs) < 0)
			{
				GKUI.TfmGEDKeeper.Instance.Options.NameFilters.Add(fs);
			}

			fs = this.cbResidence.Text.Trim();
			if (fs != "" && fs != "*" && GKUI.TfmGEDKeeper.Instance.Options.ResidenceFilters.IndexOf(fs) < 0)
			{
				GKUI.TfmGEDKeeper.Instance.Options.ResidenceFilters.Add(fs);
			}

			fs = this.cbEventVal.Text.Trim();
			if (fs != "" && fs != "*" && GKUI.TfmGEDKeeper.Instance.Options.EventFilters.IndexOf(fs) < 0)
			{
				GKUI.TfmGEDKeeper.Instance.Options.EventFilters.Add(fs);
			}

			this.Base.Filter.PatriarchOnly = this.CheckPatriarch.Checked;

			int life_sel = 0;
			if (this.RadioButton1.Checked) life_sel = 0;
			if (this.RadioButton2.Checked) life_sel = 1;
			if (this.RadioButton3.Checked) life_sel = 2;
			if (this.RadioButton4.Checked) life_sel = 3;

			if (this.Base.Filter.LifeMode != TGenEngine.TLifeMode.lmTimeLine)
			{
				this.Base.Filter.AliveBeforeDate = this.edAliveBeforeDate.Text;
				if (life_sel == 3)
				{
					try
					{
						DateTime dt = DateTime.Parse(this.edAliveBeforeDate.Text);
					}
					catch
					{
						SysUtils.ShowError(GKL.LSList[532]);
						base.DialogResult = DialogResult.None;
					}
				}
				this.Base.Filter.LifeMode = (TGenEngine.TLifeMode)life_sel;
			}

			int sex_sel = 0;
			if (this.RadioButton5.Checked) sex_sel = 0;
			if (this.RadioButton6.Checked) sex_sel = 1;
			if (this.RadioButton7.Checked) sex_sel = 2;
			this.Base.Filter.Sex = (TGEDCOMSex)sex_sel;

			if (this.edName.Text == "") this.edName.Text = "*";
			this.Base.Filter.Name = this.edName.Text;

			if (this.cbResidence.Text == "") this.cbResidence.Text = "*";
			this.Base.Filter.Residence = this.cbResidence.Text;

			if (this.cbEventVal.Text == "") this.cbEventVal.Text = "*";
			this.Base.Filter.EventVal = this.cbEventVal.Text;

			int selectedIndex = this.cbGroup.SelectedIndex;
			if (selectedIndex >= 0 && selectedIndex < 3) {
				this.Base.Filter.GroupMode = (TFilter.TGroupMode)this.cbGroup.SelectedIndex;
				this.Base.Filter.GroupRef = "";
			} else {
				TGEDCOMRecord rec = (this.cbGroup.Items[this.cbGroup.SelectedIndex] as TComboItem).Data as TGEDCOMRecord;
				if (rec != null) {
					this.Base.Filter.GroupMode = TFilter.TGroupMode.gmSelected;
					this.Base.Filter.GroupRef = rec.XRef;
				} else {
					this.Base.Filter.GroupMode = TFilter.TGroupMode.gmAll;
					this.Base.Filter.GroupRef = "";
				}
			}

			int selectedIndex2 = this.cbSource.SelectedIndex;
			if (selectedIndex2 >= 0 && selectedIndex2 < 3) {
				this.Base.Filter.SourceMode = (TFilter.TGroupMode)this.cbSource.SelectedIndex;
				this.Base.Filter.SourceRef = "";
			} else {
				TGEDCOMRecord rec = (this.cbSource.Items[this.cbSource.SelectedIndex] as TComboItem).Data as TGEDCOMRecord;
				if (rec != null) {
					this.Base.Filter.SourceMode = TFilter.TGroupMode.gmSelected;
					this.Base.Filter.SourceRef = rec.XRef;
				} else {
					this.Base.Filter.SourceMode = TFilter.TGroupMode.gmAll;
					this.Base.Filter.SourceRef = "";
				}
			}

			this.Base.ApplyFilter();
			base.DialogResult = DialogResult.OK;
		}

		private void TfmFilter_Load(object sender, EventArgs e)
		{
			this.edName.Items.AddRange(this.StringsToArray(GKUI.TfmGEDKeeper.Instance.Options.NameFilters));
			this.cbResidence.Items.AddRange(this.StringsToArray(GKUI.TfmGEDKeeper.Instance.Options.ResidenceFilters));
			this.cbEventVal.Items.AddRange(this.StringsToArray(GKUI.TfmGEDKeeper.Instance.Options.EventFilters));

			int life_sel;
			if (this.Base.Filter.LifeMode != TGenEngine.TLifeMode.lmTimeLine)
			{
				life_sel = (int)this.Base.Filter.LifeMode;
				this.rgLife.Enabled = true;
				this.edAliveBeforeDate.Text = this.Base.Filter.AliveBeforeDate;
			} else {
				life_sel = -1;
				this.rgLife.Enabled = false;
				this.edAliveBeforeDate.Text = "";
			}

			switch (life_sel) {
				case 0:
					this.RadioButton1.Checked = true;
					break;
				case 1:
					this.RadioButton2.Checked = true;
					break;
				case 2:
					this.RadioButton3.Checked = true;
					break;
				case 3:
					this.RadioButton4.Checked = true;
					break;
			}

			int sex_sel = (int)this.Base.Filter.Sex;
			switch (sex_sel) {
				case 0:
					this.RadioButton5.Checked = true;
					break;
				case 1:
					this.RadioButton6.Checked = true;
					break;
				case 2:
					this.RadioButton7.Checked = true;
					break;
			}

			this.edName.Text = this.Base.Filter.Name;
			this.cbResidence.Text = this.Base.Filter.Residence;
			this.cbEventVal.Text = this.Base.Filter.EventVal;
			this.CheckPatriarch.Checked = this.Base.Filter.PatriarchOnly;

			TGEDCOMTree tree = this.Base.Tree;

			this.cbGroup.Sorted = true;
			int num = tree.RecordsCount - 1;
			for (int i = 0; i <= num; i++) {
				if (tree.GetRecord(i) is TGEDCOMGroupRecord) {
					this.cbGroup.Items.Add(new TComboItem((tree.GetRecord(i) as TGEDCOMGroupRecord).GroupName, tree.GetRecord(i)));
				}
			}
			this.cbGroup.Sorted = false;
			this.cbGroup.Items.Insert(0, new TComboItem(GKL.LSList[500], null));
			this.cbGroup.Items.Insert(1, new TComboItem(GKL.LSList[501], null));
			this.cbGroup.Items.Insert(2, new TComboItem(GKL.LSList[502], null));
			if (this.Base.Filter.GroupMode != TFilter.TGroupMode.gmSelected)
			{
				this.cbGroup.SelectedIndex = (int)((sbyte)this.Base.Filter.GroupMode);
			} else {
				this.cbGroup.SelectedIndex = this.cbGroup.Items.IndexOf(tree.XRefIndex_Find(this.Base.Filter.GroupRef));
			}

			this.cbSource.Sorted = true;
			for (int i = 0; i <= tree.RecordsCount - 1; i++) {
				if (tree.GetRecord(i) is TGEDCOMSourceRecord) {
					this.cbSource.Items.Add(new TComboItem((tree.GetRecord(i) as TGEDCOMSourceRecord).FiledByEntry, tree.GetRecord(i)));
				}
			}
			this.cbSource.Sorted = false;
			this.cbSource.Items.Insert(0, new TComboItem(GKL.LSList[500], null));
			this.cbSource.Items.Insert(1, new TComboItem(GKL.LSList[501], null));
			this.cbSource.Items.Insert(2, new TComboItem(GKL.LSList[502], null));
			if (this.Base.Filter.SourceMode != TFilter.TGroupMode.gmSelected)
			{
				this.cbSource.SelectedIndex = (int)((sbyte)this.Base.Filter.SourceMode);
			} else {
				this.cbSource.SelectedIndex = this.cbSource.Items.IndexOf(tree.XRefIndex_Find(this.Base.Filter.SourceRef));
			}
		}

		public TfmFilter(TfmBase aBase)
		{
			this.InitializeComponent();
			this.FBase = aBase;
			this.SetLang();
		}

		public void SetLang()
		{
			this.Text = GKL.GetLS(LSID.LSID_MIFilter);
			this.btnAccept.Text = GKL.LSList[97];
			this.btnCancel.Text = GKL.LSList[98];
			this.RadioButton1.Text = GKL.LSList[522];
			this.RadioButton2.Text = GKL.LSList[523];
			this.RadioButton3.Text = GKL.LSList[524];
			this.RadioButton4.Text = GKL.LSList[525].ToLower();
			this.RadioButton5.Text = GKL.LSList[522];
			this.RadioButton6.Text = GKL.LSList[526];
			this.RadioButton7.Text = GKL.LSList[527];
			this.Label2.Text = GKL.LSList[525] + ":";
			this.Label1.Text = GKL.LSList[528];
			this.Label3.Text = GKL.LSList[529];
			this.Label6.Text = GKL.LSList[530];
			this.Label4.Text = GKL.LSList[58];
			this.Label5.Text = GKL.LSList[56];
			this.CheckPatriarch.Text = GKL.LSList[531];
		}
	}
}
