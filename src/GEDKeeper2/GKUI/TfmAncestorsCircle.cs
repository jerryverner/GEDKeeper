using System;
using System.Collections.Generic;
using System.Windows.Forms;

using GKCommon.GEDCOM;
using GKCore;
using GKCore.Interfaces;
using GKUI.Charts;

namespace GKUI
{
	public partial class TfmAncestorsCircle : Form, IChartWindow
	{
		private AncestorsCircle fAncestorsCircle;
		private IBaseWindow fBase;

		public IBaseWindow Base
		{
			get { return this.fBase; }
		}

		public AncestorsCircleOptions Options
		{
			get { return this.fAncestorsCircle.Options; }
		}

		public TfmAncestorsCircle(IBaseWindow aBase, GEDCOMIndividualRecord startPerson)
		{
			this.InitializeComponent();
			this.MdiParent = TfmGEDKeeper.Instance;
			this.ShowInTaskbar = true;

			this.fBase = aBase;

			this.fAncestorsCircle = new AncestorsCircle(this.fBase.Tree);
			this.fAncestorsCircle.Dock = DockStyle.Fill;
			this.fAncestorsCircle.NavRefresh += Chart_NavRefresh;
			this.fAncestorsCircle.RootChanged += Chart_RootChanged;

			this.fAncestorsCircle.RootPerson = startPerson;

			this.Controls.Add(this.fAncestorsCircle);

			this.SetLang();
		}

		private void miOptions_Click(object sender, EventArgs e)
		{
			using (ACOptionsDialog dlg = new ACOptionsDialog()) {
				dlg.Options = this.fAncestorsCircle.Options;

				if (dlg.ShowDialog() == DialogResult.OK) {
					this.fAncestorsCircle.Invalidate();
				}
			}
		}

		private void Chart_NavRefresh(object sender, EventArgs e)
		{
			TfmGEDKeeper.Instance.UpdateControls(false);
		}

		private void Chart_RootChanged(object sender, GEDCOMIndividualRecord person)
		{
			TfmGEDKeeper.Instance.UpdateControls(false);
		}

		#region ILocalization implementation
		
		public void SetLang()
		{
			this.Text = LangMan.LS(LSID.LSID_AncestorsCircle);
		}

		#endregion

		#region IChartWindow implementation
		
		public void GenChart(bool show)
		{
			if (show) base.Show();

			TfmGEDKeeper.Instance.UpdateControls(false);
		}
		
		#endregion

		#region IWorkWindow implementation

		public string GetStatusString()
		{
			return string.Format(LangMan.LS(LSID.LSID_TreeIndividualsCount), fAncestorsCircle.IndividualsCount.ToString());
		}

		public bool NavCanBackward()
		{
			return this.fAncestorsCircle.NavCanBackward();
		}

		public bool NavCanForward()
		{
			return this.fAncestorsCircle.NavCanForward();
		}

		public void NavNext()
		{
			this.fAncestorsCircle.NavNext();
		}

		public void NavPrev()
		{
			this.fAncestorsCircle.NavPrev();
		}

		public IList<ISearchResult> FindAll(string searchPattern)
		{
			return null;
		}

		public void QuickFind()
		{
		}

		public void SelectByRec(GEDCOMIndividualRecord iRec)
		{
		}

		public void SetFilter()
		{
		}

		#endregion
	}
}
