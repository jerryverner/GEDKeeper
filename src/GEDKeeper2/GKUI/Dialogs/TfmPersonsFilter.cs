﻿using System;
using System.Windows.Forms;

using ExtUtils;
using GedCom551;
using GKCore;
using GKCore.Interfaces;
using GKCore.Options;
using GKUI.Controls;
using GKUI.Lists;

/// <summary>
/// Localization: dirty
/// </summary>

namespace GKUI.Dialogs
{
    public partial class TfmPersonsFilter : TfmComFilter
    {
    	private readonly TIndividualListMan fListMan;
    	
        public TfmPersonsFilter(IBase aBase, TListManager aListMan) : base(aBase, aListMan)
        {
            InitializeComponent();
			this.SetSpecificLang();

            fListMan = (TIndividualListMan)aListMan;
            UpdateSpecific();
            PageControl1.SelectedIndex = 1;
        }

		public void SetSpecificLang()
		{
			this.Text = LangMan.LS(LSID.LSID_MIFilter);
			this.RadioButton1.Text = LangMan.LS(LSID.LSID_All);
			this.RadioButton2.Text = LangMan.LS(LSID.LSID_OnlyAlive);
			this.RadioButton3.Text = LangMan.LS(LSID.LSID_OnlyDied);
			this.RadioButton4.Text = LangMan.LS(LSID.LSID_AliveBefore).ToLower();
			this.RadioButton5.Text = LangMan.LS(LSID.LSID_All);
			this.RadioButton6.Text = LangMan.LS(LSID.LSID_OnlyMans);
			this.RadioButton7.Text = LangMan.LS(LSID.LSID_OnlyWomans);
			this.Label2.Text = LangMan.LS(LSID.LSID_AliveBefore) + ":";
			this.Label1.Text = LangMan.LS(LSID.LSID_NameMask);
			this.Label3.Text = LangMan.LS(LSID.LSID_PlaceMask);
			this.Label6.Text = LangMan.LS(LSID.LSID_EventMask);
			this.Label4.Text = LangMan.LS(LSID.LSID_RPGroups);
			this.Label5.Text = LangMan.LS(LSID.LSID_RPSources);
			this.CheckPatriarch.Text = LangMan.LS(LSID.LSID_OnlyPatriarchs);
		}

		private void rgLifeClick(object sender, EventArgs e)
		{
			this.edAliveBeforeDate.Enabled = this.RadioButton4.Checked;
		}

		public override void DoReset()
		{
			base.DoReset();
			this.UpdateSpecific();
		}
		
        private void UpdateSpecific()
        {
        	TIndividualListFilter iFilter = (TIndividualListFilter)fListMan.Filter;
            GlobalOptions options = TfmGEDKeeper.Instance.Options;
        	
        	this.edName.Items.Clear();
            this.edName.Items.AddRange(options.NameFilters.ToArray());
			this.edName.Items.Insert(0, "*");

			this.cbResidence.Items.Clear();
            this.cbResidence.Items.AddRange(options.ResidenceFilters.ToArray());
			this.cbResidence.Items.Insert(0, "*");

			this.cbEventVal.Items.Clear();
            this.cbEventVal.Items.AddRange(options.EventFilters.ToArray());

			int lifeSel;
			if (iFilter.LifeMode != TLifeMode.lmTimeLocked)
			{
				lifeSel = (int)iFilter.LifeMode;
				this.rgLife.Enabled = true;
				this.edAliveBeforeDate.Text = iFilter.AliveBeforeDate;
			} else {
				lifeSel = -1;
				this.rgLife.Enabled = false;
				this.edAliveBeforeDate.Text = "";
			}

			switch (lifeSel) {
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

			int sexSel = (int)iFilter.Sex;
			switch (sexSel) {
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

			this.edName.Text = iFilter.Name;
			this.cbResidence.Text = iFilter.Residence;
			this.cbEventVal.Text = iFilter.EventVal;
			this.CheckPatriarch.Checked = iFilter.PatriarchOnly;

			TGEDCOMTree tree = this.Base.Tree;

			this.cbGroup.Items.Clear();
			this.cbGroup.Sorted = true;
			int num = tree.RecordsCount - 1;
			for (int i = 0; i <= num; i++) {
				TGEDCOMRecord rec = tree[i];
				if (rec is TGEDCOMGroupRecord) {
					this.cbGroup.Items.Add(new GKComboItem((rec as TGEDCOMGroupRecord).GroupName, rec));
				}
			}
			this.cbGroup.Sorted = false;
			this.cbGroup.Items.Insert(0, new GKComboItem(LangMan.LS(LSID.LSID_SrcAll), null));
			this.cbGroup.Items.Insert(1, new GKComboItem(LangMan.LS(LSID.LSID_SrcNot), null));
			this.cbGroup.Items.Insert(2, new GKComboItem(LangMan.LS(LSID.LSID_SrcAny), null));
			if (iFilter.GroupMode != TGroupMode.gmSelected) {
				this.cbGroup.SelectedIndex = (int)iFilter.GroupMode;
			} else {
			    TGEDCOMGroupRecord groupRec = tree.XRefIndex_Find(iFilter.GroupRef) as TGEDCOMGroupRecord;
                this.cbGroup.Text = groupRec.GroupName;
			}

			this.cbSource.Items.Clear();
			this.cbSource.Sorted = true;
			for (int i = 0; i <= tree.RecordsCount - 1; i++) {
				TGEDCOMRecord rec = tree[i];
				if (rec is TGEDCOMSourceRecord) {
					this.cbSource.Items.Add(new GKComboItem((rec as TGEDCOMSourceRecord).FiledByEntry, rec));
				}
			}
			this.cbSource.Sorted = false;
			this.cbSource.Items.Insert(0, new GKComboItem(LangMan.LS(LSID.LSID_SrcAll), null));
			this.cbSource.Items.Insert(1, new GKComboItem(LangMan.LS(LSID.LSID_SrcNot), null));
			this.cbSource.Items.Insert(2, new GKComboItem(LangMan.LS(LSID.LSID_SrcAny), null));
			if (iFilter.SourceMode != TGroupMode.gmSelected) {
				this.cbSource.SelectedIndex = (int)iFilter.SourceMode;
			} else {
			    TGEDCOMSourceRecord sourceRec = tree.XRefIndex_Find(iFilter.SourceRef) as TGEDCOMSourceRecord;
                this.cbSource.Text = sourceRec.FiledByEntry;
			}
        }
        
        private static void SaveFilter(string flt, StringList filters)
        {
            if (flt != "" && flt != "*" && filters.IndexOf(flt) < 0) filters.Add(flt);
        }

		public override void AcceptChanges()
		{
			base.AcceptChanges();

			TIndividualListFilter iFilter = (TIndividualListFilter)fListMan.Filter;
			
			string fs = this.edName.Text.Trim();
            SaveFilter(fs, TfmGEDKeeper.Instance.Options.NameFilters);

			fs = this.cbResidence.Text.Trim();
            SaveFilter(fs, TfmGEDKeeper.Instance.Options.ResidenceFilters);

			fs = this.cbEventVal.Text.Trim();
            SaveFilter(fs, TfmGEDKeeper.Instance.Options.EventFilters);

			iFilter.PatriarchOnly = this.CheckPatriarch.Checked;

			int lifeSel = 0;
			if (this.RadioButton1.Checked) lifeSel = 0;
			if (this.RadioButton2.Checked) lifeSel = 1;
			if (this.RadioButton3.Checked) lifeSel = 2;
			if (this.RadioButton4.Checked) lifeSel = 3;

			if (iFilter.LifeMode != TLifeMode.lmTimeLocked)
			{
				iFilter.AliveBeforeDate = this.edAliveBeforeDate.Text;
				if (lifeSel == 3)
				{
					try
					{
						/*DateTime dt = */
                        DateTime.Parse(this.edAliveBeforeDate.Text);
					}
					catch
					{
						GKUtils.ShowError(LangMan.LS(LSID.LSID_DateInvalid));
						base.DialogResult = DialogResult.None;
					}
				}
				iFilter.LifeMode = (TLifeMode)lifeSel;
			}

			int sexSel = 0;
			if (this.RadioButton5.Checked) sexSel = 0;
			if (this.RadioButton6.Checked) sexSel = 1;
			if (this.RadioButton7.Checked) sexSel = 2;
			iFilter.Sex = (TGEDCOMSex)sexSel;

			if (this.edName.Text == "") this.edName.Text = "*";
			iFilter.Name = this.edName.Text;

			if (this.cbResidence.Text == "") this.cbResidence.Text = "*";
			iFilter.Residence = this.cbResidence.Text;

			if (this.cbEventVal.Text == "") this.cbEventVal.Text = "*";
			iFilter.EventVal = this.cbEventVal.Text;

			int selectedIndex = this.cbGroup.SelectedIndex;
			if (selectedIndex >= 0 && selectedIndex < 3) {
				iFilter.GroupMode = (TGroupMode)this.cbGroup.SelectedIndex;
				iFilter.GroupRef = "";
			} else {
			    GKComboItem item = this.cbGroup.Items[this.cbGroup.SelectedIndex] as GKComboItem;
				TGEDCOMRecord rec = item.Data as TGEDCOMRecord;
				if (rec != null) {
					iFilter.GroupMode = TGroupMode.gmSelected;
					iFilter.GroupRef = rec.XRef;
				} else {
					iFilter.GroupMode = TGroupMode.gmAll;
					iFilter.GroupRef = "";
				}
			}

			int selectedIndex2 = this.cbSource.SelectedIndex;
			if (selectedIndex2 >= 0 && selectedIndex2 < 3) {
				iFilter.SourceMode = (TGroupMode)this.cbSource.SelectedIndex;
				iFilter.SourceRef = "";
			} else {
			    GKComboItem item = this.cbSource.Items[this.cbSource.SelectedIndex] as GKComboItem;
				TGEDCOMRecord rec = item.Data as TGEDCOMRecord;
				if (rec != null) {
					iFilter.SourceMode = TGroupMode.gmSelected;
					iFilter.SourceRef = rec.XRef;
				} else {
					iFilter.SourceMode = TGroupMode.gmAll;
					iFilter.SourceRef = "";
				}
			}
		}
    }
}
