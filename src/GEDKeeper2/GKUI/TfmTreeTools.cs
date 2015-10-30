﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

using GKCommon;
using GKCommon.GEDCOM;
using GKCommon.GEDCOM.Enums;
using GKCore;
using GKCore.Interfaces;
using GKCore.Tools;
using GKCore.Types;
using GKUI.Controls;

namespace GKUI
{
    /// <summary>
    /// Localization: dirty
    /// </summary>
    public sealed partial class TfmTreeTools : Form
	{
		private static readonly string[] fHelpTopics;

		// runtime
		private readonly IBaseWindow fBase;
		private readonly GEDCOMTree fTree;

        private readonly List<GEDCOMRecord> fSplitList;
		private GEDCOMRecordType fRMMode;
		private readonly StringList fRMSkip;
		private int fRMIndex;
		private readonly StringList fPlaces;
		private readonly ExtList<TreeTools.TCheckObj> fChecksList;

		// UI
		private GKListView ListPlaces;
		private GKListView ListChecks;
		private GKListView ListPatriarchs;


		public IBaseWindow Base
		{
			get	{ return this.fBase; }
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				this.fChecksList.Dispose();
				TreeTools.PlacesSearch_Clear(this.fPlaces);
                this.fPlaces.Dispose();
                this.fRMSkip.Dispose();
				//this.fSplitList.Dispose();
			}
			base.Dispose(disposing);
		}

		public TfmTreeTools(IBaseWindow aBase)
		{
			this.InitializeComponent();

			this.fBase = aBase;
			this.fTree = this.Base.Tree;

			this.PageControl.SelectedIndex = 0;

            this.fSplitList = new List<GEDCOMRecord>();
			this.fRMSkip = new StringList();
			this.fRMMode = GEDCOMRecordType.rtIndividual;

			this.MergeCtl.Base = this.fBase;
			this.MergeCtl.MergeMode = this.fRMMode;

			this.fPlaces = new StringList();
			this.fPlaces.Sorted = true;
			this.fChecksList = new ExtList<TreeTools.TCheckObj>(true);

			this.PrepareChecksList();
			this.PreparePatriarchsList();
			this.PreparePlacesList();

			this.SetLang();
		}

		public void SetLang()
		{
			this.Text = LangMan.LS(LSID.LSID_MITreeTools);

			this.SheetTreeCompare.Text = LangMan.LS(LSID.LSID_ToolOp_1);
			this.SheetTreeMerge.Text = LangMan.LS(LSID.LSID_ToolOp_2);
			this.SheetTreeSplit.Text = LangMan.LS(LSID.LSID_ToolOp_3);
			this.SheetRecMerge.Text = LangMan.LS(LSID.LSID_ToolOp_4);
			this.SheetFamilyGroups.Text = LangMan.LS(LSID.LSID_ToolOp_6);
			this.SheetTreeCheck.Text = LangMan.LS(LSID.LSID_ToolOp_7);
			this.SheetPatSearch.Text = LangMan.LS(LSID.LSID_ToolOp_8);
			this.SheetPlaceManage.Text = LangMan.LS(LSID.LSID_ToolOp_9);
			
			//this.SheetMerge.Text
			//this.SheetOptions.Text

			this.btnClose.Text = LangMan.LS(LSID.LSID_DlgClose);
			this.btnHelp.Text = LangMan.LS(LSID.LSID_MIHelp);
			this.Label1.Text = LangMan.LS(LSID.LSID_MIFile);
			
			this.btnFileChoose.Text = LangMan.LS(LSID.LSID_DlgSelect) + "...";
			this.btnTreeMerge.Text = LangMan.LS(LSID.LSID_DlgSelect) + "...";
			
			this.btnSelectAll.Text = LangMan.LS(LSID.LSID_SelAll);
			this.btnSelectFamily.Text = LangMan.LS(LSID.LSID_SelFamily);
			this.btnSelectAncestors.Text = LangMan.LS(LSID.LSID_SelAncestors);
			this.btnSelectDescendants.Text = LangMan.LS(LSID.LSID_SelDescendants);
			this.btnDelete.Text = LangMan.LS(LSID.LSID_DoDelete);
			this.btnSave.Text = LangMan.LS(LSID.LSID_MIFileSave);
			this.SheetMerge.Text = LangMan.LS(LSID.LSID_RecMerge);
			this.SheetOptions.Text = LangMan.LS(LSID.LSID_MIOptions);
			this.btnSearch.Text = LangMan.LS(LSID.LSID_RM_Search);
			this.btnSkip.Text = LangMan.LS(LSID.LSID_RM_Skip);
			this.rgMode.Text = LangMan.LS(LSID.LSID_RM_Records);
			this.RadioButton5.Text = LangMan.LS(LSID.LSID_RPIndividuals);
			this.RadioButton6.Text = LangMan.LS(LSID.LSID_RPNotes);
			this.RadioButton7.Text = LangMan.LS(LSID.LSID_RPFamilies);
			this.RadioButton8.Text = LangMan.LS(LSID.LSID_RPSources);
			this.GroupBox1.Text = LangMan.LS(LSID.LSID_RM_SearchPersons);
			this.chkIndistinctMatching.Text = LangMan.LS(LSID.LSID_RM_IndistinctMatching);
			this.chkBirthYear.Text = LangMan.LS(LSID.LSID_RM_BirthYear);
			this.Label5.Text = LangMan.LS(LSID.LSID_RM_NameAccuracy);
			this.Label6.Text = LangMan.LS(LSID.LSID_RM_YearInaccuracy);
			this.btnBaseRepair.Text = LangMan.LS(LSID.LSID_Repair);
			this.Label8.Text = LangMan.LS(LSID.LSID_MinGenerations);
			this.btnSetPatriarch.Text = LangMan.LS(LSID.LSID_SetPatFlag);
			this.btnPatSearch.Text = LangMan.LS(LSID.LSID_Search);
			this.btnIntoList.Text = LangMan.LS(LSID.LSID_InsertIntoBook);
		}

		static TfmTreeTools()
		{
			fHelpTopics = new string[]
			{
				"::/gkhTools_TreeCompare.html", 
				"::/gkhTools_TreeMerge.html", 
				"::/gkhTools_TreeSplit.html", 
				"::/gkhTools_DubsMerge.html", 
				//"::/gkhTools_TreeImport.html", 
				"::/gkhTools_FamiliesConnectivity.html", 
				"::/gkhTools_TreeCheck.html", 
				"::/gkhTools_PatSearch.html", 
				"::/gkhTools_PlacesManage.html"
			};
		}

		void btnHelp_Click(object sender, EventArgs e)
		{
			TfmGEDKeeper.Instance.ShowHelpTopic(fHelpTopics[this.PageControl.SelectedIndex]);
		}

		void PageControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (this.PageControl.SelectedTab == this.SheetFamilyGroups)
			{
				this.CheckGroups();
			}
			else if (this.PageControl.SelectedTab == this.SheetTreeCheck)
			{
				this.CheckBase();
			}
			else if (this.PageControl.SelectedTab == this.SheetPlaceManage)
			{
				this.CheckPlaces();
			}
		}

		#region TreeMerge

		void btnTreeMerge_Click(object sender, EventArgs e)
		{
			if (this.OpenDialog1.ShowDialog() == DialogResult.OK) {
				this.edUpdateBase.Text = this.OpenDialog1.FileName;

				TreeTools.TreeMerge(this.Base.Tree, this.edUpdateBase.Text, this.mSyncRes);

				this.Base.RefreshLists(false);
			}
		}

		#endregion

		#region Duplicates Search

		private void SearchDups()
		{
			this.MergeCtl.Base = this.fBase;
			this.MergeCtl.MergeMode = this.fRMMode;
			
			this.MergeCtl.SetRec1(null);
			this.MergeCtl.SetRec2(null);
			
			MatchParams mParams;
			//mParams.IndistinctNameMatching = this.chkIndistinctMatching.Checked;
			mParams.NamesIndistinctThreshold = (float)decimal.ToDouble(this.edNameAccuracy.Value) / 100.0f;
			mParams.DatesCheck = this.chkBirthYear.Checked;
			mParams.YearsInaccuracy = decimal.ToInt32(this.edYearInaccuracy.Value);
			mParams.RusNames = true;

			bool res = false;
			this.btnSkip.Enabled = false;

			try
			{
				this.ProgressBar1.Minimum = 0;
				this.ProgressBar1.Maximum = this.fTree.RecordsCount;
				this.ProgressBar1.Value = this.fRMIndex;

				int num = this.fTree.RecordsCount - 1;
				for (int i = this.fRMIndex; i <= num; i++)
				{
					this.fRMIndex = i;
					this.ProgressBar1.Increment(1);

					GEDCOMRecord iRec = this.fTree[i];
					if (iRec.RecordType != this.fRMMode) continue;

					int num5 = this.fTree.RecordsCount - 1;
					for (int j = i + 1; j <= num5; j++)
					{
						GEDCOMRecord kRec = this.fTree[j];
						if (kRec.RecordType != this.fRMMode) continue;

						if (iRec == kRec) continue;
						if (this.fRMSkip.IndexOf(iRec.XRef + "-" + kRec.XRef) >= 0) continue;

						res = iRec.IsMatch(kRec, mParams) >= 100.0f;

						if (res) {
							this.MergeCtl.SetRec1(iRec);
							this.MergeCtl.SetRec2(kRec);
							break;
						}
					}

					if (res) break;
				}
			}
			finally
			{
				this.btnSkip.Enabled = true;
			}
		}

		void RadioButton8_Click(object sender, EventArgs e)
		{
			if (this.RadioButton5.Checked) this.fRMMode = GEDCOMRecordType.rtIndividual;
			if (this.RadioButton6.Checked) this.fRMMode = GEDCOMRecordType.rtNote;
			if (this.RadioButton7.Checked) this.fRMMode = GEDCOMRecordType.rtFamily;
			if (this.RadioButton8.Checked) this.fRMMode = GEDCOMRecordType.rtSource;

			this.MergeCtl.MergeMode = this.fRMMode;
		}

		void btnSkip_Click(object sender, EventArgs e)
		{
			if (this.MergeCtl.Rec1 != null && this.MergeCtl.Rec2 != null)
			{
				this.fRMSkip.Add(this.MergeCtl.Rec1.XRef + "-" + this.MergeCtl.Rec2.XRef);
			}
			this.SearchDups();
		}

		void btnSearch_Click(object sender, EventArgs e)
		{
			this.fRMIndex = 0;
			this.fRMSkip.Clear();
			this.SearchDups();
		}

		#endregion

		#region CheckGroups

		private void CheckGroups()
		{
			gkLogChart1.Clear();
			fBase.ProgressInit(LangMan.LS(LSID.LSID_CheckFamiliesConnection), this.fTree.RecordsCount);
			ExtList<GEDCOMIndividualRecord> prepared = new ExtList<GEDCOMIndividualRecord>();
			try
			{
				int group = 0;
				this.TreeView1.Nodes.Clear();

				int num = this.fTree.RecordsCount;
				for (int i = 0; i < num; i++)
				{
					GEDCOMRecord rec = this.fTree[i];

					if (rec is GEDCOMIndividualRecord)
					{
						GEDCOMIndividualRecord iRec = rec as GEDCOMIndividualRecord;
						if (prepared.IndexOf(iRec) < 0)
						{
							group++;
							this.fSplitList.Clear();

							TreeTools.TreeWalk(iRec, TreeTools.TreeWalkMode.twmAll, this.fSplitList);

							TreeNode root = this.TreeView1.Nodes.Add(
								group.ToString() + " " + LangMan.LS(LSID.LSID_Group).ToLower() + " (" + this.fSplitList.Count.ToString() + ")");

							int cnt = this.fSplitList.Count;
							int num2 = cnt - 1;
							for (int j = 0; j <= num2; j++)
							{
								iRec = this.fSplitList[j] as GEDCOMIndividualRecord;
								prepared.Add(iRec);
								string pn = iRec.GetNameString(true, false);
								if (iRec.Patriarch)
								{
									pn = "(*) " + pn;
								}
								root.Nodes.Add(new GKTreeNode(pn, iRec));
							}
							root.ExpandAll();
							
							gkLogChart1.AddFragment(cnt);
						}
					}

					fBase.ProgressStep();
					Application.DoEvents();
				}
			}
			finally
			{
				this.fSplitList.Clear();
				prepared.Dispose();
				fBase.ProgressDone();
			}
		}

		void TreeView1_DoubleClick(object sender, EventArgs e)
		{
			GKTreeNode node = this.TreeView1.SelectedNode as GKTreeNode;
		    if (node == null) return;
		    
            GEDCOMIndividualRecord iRec = node.Data as GEDCOMIndividualRecord;
		    if (iRec == null) return;
		    
            this.Base.SelectRecordByXRef(iRec.XRef);
		    base.Close();
		}

		#endregion

		#region Tree Verify

		private void PrepareChecksList()
		{
			this.ListChecks = GKUtils.CreateListView(this.Panel1);
			this.ListChecks.CheckBoxes = true;
			this.ListChecks.DoubleClick += this.ListChecksDblClick;
			this.ListChecks.AddListColumn(LangMan.LS(LSID.LSID_Record), 400, false);
			this.ListChecks.AddListColumn(LangMan.LS(LSID.LSID_Problem), 200, false);
			this.ListChecks.AddListColumn(LangMan.LS(LSID.LSID_Solve), 200, false);
		}

		private void CheckBase()
		{
			TreeTools.CheckBase(this.fBase, this.fChecksList);

			this.ListChecks.Items.Clear();

			int num2 = this.fChecksList.Count;
			for (int i = 0; i < num2; i++)
			{
				TreeTools.TCheckObj checkObj = this.fChecksList[i] as TreeTools.TCheckObj;

				ListViewItem item = this.ListChecks.AddItem(checkObj.GetRecordName(), checkObj);
				item.SubItems.Add(checkObj.Comment);
				item.SubItems.Add(LangMan.LS(GKData.CheckSolveNames[(int)checkObj.Solve]));
			}

			this.ListChecks.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
		}

		void btnBaseRepair_Click(object sender, EventArgs e)
		{
			try
			{
				int num = this.ListChecks.Items.Count;
				for (int i = 0; i < num; i++)
				{
					GKListItem item = this.ListChecks.Items[i] as GKListItem;

					if (item.Checked) {
						TreeTools.TCheckObj checkObj = item.Data as TreeTools.TCheckObj;
						TreeTools.RepairProblem(this.fBase, checkObj);
					}
				}
			}
			finally
			{
				this.Base.RefreshLists(false);
				this.CheckBase();
			}
		}

		void ListChecksDblClick(object sender, EventArgs e)
		{
			GKListItem item = this.ListChecks.SelectedItem();
			if (item != null)
			{
				GEDCOMIndividualRecord i_rec = (item.Data as TreeTools.TCheckObj).Rec as GEDCOMIndividualRecord;
				if (i_rec != null)
				{
					this.Base.SelectRecordByXRef(i_rec.XRef);
					this.Close();
				}
			}
		}

		#endregion

		#region Places Management

		private void PreparePlacesList()
		{
			this.ListPlaces = GKUtils.CreateListView(this.Panel4);
			this.ListPlaces.DoubleClick += this.ListPlacesDblClick;
			this.ListPlaces.AddListColumn(LangMan.LS(LSID.LSID_Place), 400, false);
			this.ListPlaces.AddListColumn(LangMan.LS(LSID.LSID_LinksCount), 100, false);
		}

		private void CheckPlaces()
		{
			this.ListPlaces.BeginUpdate();
			try
			{
				TreeTools.PlacesSearch(this.fTree, this.fPlaces, fBase);

				this.ListPlaces.Items.Clear();

				int num4 = this.fPlaces.Count;
				for (int i = 0; i < num4; i++)
				{
					PlaceObj place_obj = this.fPlaces.GetObject(i) as PlaceObj;

					GKListItem item = this.ListPlaces.AddItem(this.fPlaces[i], place_obj);
					item.SubItems.Add(place_obj.Facts.Count.ToString());
				}
			}
			finally
			{
				this.ListPlaces.EndUpdate();
			}
		}

		void btnIntoList_Click(object sender, EventArgs e)
		{
			this.ListPlacesDblClick(null, null);
		}

		void ListPlacesDblClick(object sender, EventArgs e)
		{
			GKListItem item = this.ListPlaces.SelectedItem();
			if (item != null)
			{
				PlaceObj p_obj = item.Data as PlaceObj;
				if (p_obj != null)
				{
					if (p_obj.Name.IndexOf("[*]") == 0)
					{
						GKUtils.ShowMessage(LangMan.LS(LSID.LSID_PlaceAlreadyInBook));
					}
					else
					{
						GEDCOMLocationRecord loc = this.Base.SelectRecord(GEDCOMRecordType.rtLocation, new object[] { p_obj.Name }) as GEDCOMLocationRecord;

						if (loc != null)
						{
							int num = p_obj.Facts.Count;
							for (int i = 0; i < num; i++)
							{
								GEDCOMCustomEvent evt = p_obj.Facts[i] as GEDCOMCustomEvent;
								evt.Detail.Place.StringValue = loc.LocationName;
								evt.Detail.Place.Location.Value = loc;
							}

							this.CheckPlaces();

							this.Base.RefreshLists(false);
						}
					}
				}
			}
		}

		#endregion

		#region Tree Splitting

		private void UpdateSplitLists()
		{
			this.ListSelected.BeginUpdate();
			this.ListSelected.Items.Clear();
			this.ListSkipped.BeginUpdate();
			this.ListSkipped.Items.Clear();
			try
			{
				int cnt = 0;

				int num = this.fTree.RecordsCount - 1;
				for (int i = 0; i <= num; i++)
				{
					GEDCOMRecord rec = this.fTree[i];
					if (rec is GEDCOMIndividualRecord)
					{
						cnt++;
						GEDCOMIndividualRecord i_rec = rec as GEDCOMIndividualRecord;
						string st = i_rec.XRef + " / " + i_rec.GetNameString(true, false);

						if (this.fSplitList.IndexOf(i_rec) < 0) {
							this.ListSkipped.Items.Add(st);
						} else {
							this.ListSelected.Items.Add(st);
						}
					}
				}
				this.Text = this.fSplitList.Count.ToString() + " / " + cnt.ToString();
			}
			finally
			{
				this.ListSelected.EndUpdate();
				this.ListSkipped.EndUpdate();
			}
		}

		private void Select(GEDCOMIndividualRecord startPerson, TreeTools.TreeWalkMode walkMode)
		{
			this.fSplitList.Clear();
			TreeTools.TreeWalk(startPerson, walkMode, this.fSplitList);
			this.UpdateSplitLists();
		}

		private void btnSelectFamily_Click(object sender, EventArgs e)
		{
			this.Select(this.Base.GetSelectedPerson(), TreeTools.TreeWalkMode.twmFamily);
		}

		private void btnSelectAncestors_Click(object sender, EventArgs e)
		{
			this.Select(this.Base.GetSelectedPerson(), TreeTools.TreeWalkMode.twmAncestors);
		}

		private void btnSelectDescendants_Click(object sender, EventArgs e)
		{
			this.Select(this.Base.GetSelectedPerson(), TreeTools.TreeWalkMode.twmDescendants);
		}

		private void btnSelectAll_Click(object sender, EventArgs e)
		{
			this.Select(this.Base.GetSelectedPerson(), TreeTools.TreeWalkMode.twmAll);
		}

		private void btnDelete_Click(object sender, EventArgs e)
		{
			int num = this.fSplitList.Count;
			for (int i = 0; i < num; i++)
			{
				object obj = this.fSplitList[i];

				if (obj is GEDCOMIndividualRecord)
				{
					this.Base.RecordDelete(obj as GEDCOMIndividualRecord, false);
				}
			}

			GKUtils.ShowMessage(LangMan.LS(LSID.LSID_RecsDeleted));
			this.fSplitList.Clear();
			this.UpdateSplitLists();

			this.Base.RefreshLists(false);
		}

		private void btnSave_Click(object sender, EventArgs e)
		{
			if (this.SaveDialog1.ShowDialog() == DialogResult.OK)
			{
				TreeTools.CheckRelations(fSplitList);

				string fileName = this.SaveDialog1.FileName;

				string subm = this.fTree.Header.GetTagStringValue("SUBM");
				this.fTree.Header.Clear();
				this.fTree.Header.Source = "GEDKeeper";
				this.fTree.Header.ReceivingSystemName = "GEDKeeper";
				this.fTree.Header.CharacterSet = TfmGEDKeeper.Instance.Options.DefCharacterSet;
				this.fTree.Header.Language = "Russian";
				this.fTree.Header.GEDCOMVersion = "5.5";
				this.fTree.Header.GEDCOMForm = "LINEAGE-LINKED";
				this.fTree.Header.FileName = Path.GetFileName(fileName);
				this.fTree.Header.TransmissionDate.Date = DateTime.Now;

				if (subm != "") {
					this.fTree.Header.SetTagStringValue("SUBM", subm);
				}

				using (StreamWriter fs = new StreamWriter(fileName, false, GEDCOMUtils.GetEncodingByCharacterSet(this.fTree.Header.CharacterSet)))
				{
					this.fTree.SaveToStream(fs, this.fSplitList);
					this.fTree.Header.CharacterSet = GEDCOMCharacterSet.csASCII;
				}
			}
		}

		#endregion

		#region Patriarchs Search

		private void PreparePatriarchsList()
		{
			this.ListPatriarchs = GKUtils.CreateListView(this.Panel3);
			this.ListPatriarchs.DoubleClick += this.ListPatriarchsDblClick;
			this.ListPatriarchs.AddListColumn(LangMan.LS(LSID.LSID_Patriarch), 400, false);
			this.ListPatriarchs.AddListColumn(LangMan.LS(LSID.LSID_Birth), 90, false);
			this.ListPatriarchs.AddListColumn(LangMan.LS(LSID.LSID_Descendants), 90, false);
			this.ListPatriarchs.AddListColumn(LangMan.LS(LSID.LSID_Generations), 90, false);
		}

		void ListPatriarchsDblClick(object sender, EventArgs e)
		{
			GKListItem item = this.ListPatriarchs.SelectedItem();
			if (item != null)
			{
				GEDCOMIndividualRecord i_rec = item.Data as GEDCOMIndividualRecord;
				if (i_rec != null)
				{
					this.Base.SelectRecordByXRef(i_rec.XRef);
					this.Close();
				}
			}
		}

		void btnPatSearch_Click(object sender, EventArgs e)
		{
			this.ListPatriarchs.BeginUpdate();
			ExtList<PatriarchObj> lst = new ExtList<PatriarchObj>(true);
			try
			{
				this.ListPatriarchs.Items.Clear();
				this.fBase.Context.GetPatriarchsList(lst, decimal.ToInt32(this.edMinGens.Value), !chkWithoutDates.Checked);

				int num = lst.Count;
				for (int i = 0; i < num; i++)
				{
					PatriarchObj p_obj = lst[i];
					string p_sign = ((p_obj.IRec.Patriarch) ? "[*] " : "");

					GKListItem item = this.ListPatriarchs.AddItem(p_sign + p_obj.IRec.GetNameString(true, false), p_obj.IRec);
					item.SubItems.Add(p_obj.BirthYear.ToString());
					item.SubItems.Add(p_obj.DescendantsCount.ToString());
					item.SubItems.Add(p_obj.DescGenerations.ToString());
				}
			}
			finally
			{
				lst.Dispose();
				this.ListPatriarchs.EndUpdate();
			}
		}

		void btnSetPatriarch_Click(object sender, EventArgs e)
		{
			try
			{
				GKListItem item = this.ListPatriarchs.SelectedItem();
				if (item != null)
				{
					GEDCOMIndividualRecord i_rec = item.Data as GEDCOMIndividualRecord;
					if (i_rec != null)
					{
						i_rec.Patriarch = true;
					}

					this.Base.RefreshLists(false);
				}
			}
			finally
			{
				this.btnPatSearch_Click(null, null);
			}
		}

		void BtnPatriarchsDiagramClick(object sender, EventArgs e)
		{
			PatriarchsViewer wnd = new PatriarchsViewer(this.fBase, decimal.ToInt32(this.edMinGens.Value));
			wnd.Show();

			//TreeTools.GenPatriarchsGraphviz(@"d:\document1.txt", this.Base.Tree, decimal.ToInt32(this.edMinGens.Value));
		}

		#endregion

		#region Match files

		private string external_match_db;
		private enum TreeMatchType { tmtInternal, tmtExternal, tmtAnalysis }

		void btnFileChoose_Click(object sender, EventArgs e)
		{
			if (this.OpenDialog1.ShowDialog() == DialogResult.OK)
			{
				external_match_db = this.OpenDialog1.FileName;
				this.edCompareFile.Text = Path.GetFileName(external_match_db);
			}
		}

		private void DuplicateFoundFunc(GEDCOMIndividualRecord indivA, GEDCOMIndividualRecord indivB)
		{
			this.ListCompare.AppendText("    * [" + indivA.GetNameString(true, false) + "]\r\n");
			this.ListCompare.AppendText("      [" + indivB.GetNameString(true, false) + "]\r\n\r\n");
			//this.ListCompare.AppendText("\r\n");
		}

		private TreeMatchType GetTreeMatchType()
		{
			TreeMatchType type = 
				((rbtnMatchInternal.Checked) ? 
				 	TreeMatchType.tmtInternal : 
				 ((rbtnMathExternal.Checked) ? TreeMatchType.tmtExternal : TreeMatchType.tmtAnalysis));

			return type;
		}

		void btnMatch_Click(object sender, EventArgs e)
		{
			TreeMatchType type = GetTreeMatchType();

			this.ListCompare.Clear();

			switch (type) {
				case TreeMatchType.tmtInternal:
					TreeTools.FindDuplicates(this.fTree, this.fTree, 90 /*min: 80-85*/, DuplicateFoundFunc, fBase);
					break;

				case TreeMatchType.tmtExternal:
					TreeTools.TreeCompare(this.fTree, external_match_db, this.ListCompare);
					break;

				case TreeMatchType.tmtAnalysis:
					{
						List<TreeTools.ULIndividual> uln = TreeTools.GetUnlinkedNamesakes(this.fTree, fBase);

						this.ListCompare.AppendText("  Поиск несвязанных однофамильцев:\r\n");
						if (uln != null && uln.Count > 0)
						{
							int num = uln.Count;
							for (int i = 0; i < num; i++)
							{
								TreeTools.ULIndividual indiv = uln[i];
								this.ListCompare.AppendText("    - [" + indiv.Family + "] " + indiv.iRec.GetNameString(true, false) + "\r\n");
							}
						}
						else
						{
							this.ListCompare.AppendText("    - not found.");
						}
						break;
					}
			}
		}

		void rbtnMatch_CheckedChanged(object sender, EventArgs e)
		{
			TreeMatchType type = GetTreeMatchType();

			this.Label1.Enabled = (type == TreeMatchType.tmtExternal);
			this.edCompareFile.Enabled = (type == TreeMatchType.tmtExternal);
			this.btnFileChoose.Enabled = (type == TreeMatchType.tmtExternal);
		}

		#endregion

	}
}
