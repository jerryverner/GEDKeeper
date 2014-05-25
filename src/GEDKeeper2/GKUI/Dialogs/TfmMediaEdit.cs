﻿using System;
using System.IO;
using System.Windows.Forms;

using ExtUtils;
using GedCom551;
using GKCore;
using GKCore.Interfaces;
using GKUI.Sheets;

/// <summary>
/// Localization: clean
/// </summary>

namespace GKUI.Dialogs
{
	public partial class TfmMediaEdit : Form, IBaseEditor
	{
		private bool fIsNew;
		private TGEDCOMMultimediaRecord fMediaRec;

        private readonly IBase fBase;
		private readonly GKNotesSheet fNotesList;
		private readonly GKSourcesSheet fSourcesList;

		public TGEDCOMMultimediaRecord MediaRec
		{
			get { return this.fMediaRec; }
			set { this.SetMediaRec(value); }
		}

		public IBase Base
		{
			get { return this.fBase; }
		}

		private bool AcceptChanges()
		{
			bool result = true;

			TGEDCOMFileReferenceWithTitle fileRef = this.fMediaRec.FileReferences[0];

			if (this.fIsNew)
			{
				MediaStoreType gst = (MediaStoreType)this.cbStoreType.SelectedIndex;
				if ((gst == MediaStoreType.mstArchive || gst == MediaStoreType.mstStorage) && !this.fBase.CheckBasePath())
				{
					return false;
				}

				result = this.fBase.MediaSave(fileRef, this.edFile.Text, gst);

				if (!result) {
					return false;
				}
			}

			fileRef.MediaType = (TGEDCOMMediaType)this.cbMediaType.SelectedIndex;
			fileRef.Title = this.edName.Text;

			this.ControlsRefresh();
			this.fBase.ChangeRecord(this.fMediaRec);

			return result;
		}

		private void ControlsRefresh()
		{
			TGEDCOMFileReferenceWithTitle fileRef = this.fMediaRec.FileReferences[0];

			this.fIsNew = (fileRef.StringValue == "");

			this.edName.Text = fileRef.Title;
			this.cbMediaType.SelectedIndex = (int)fileRef.MediaType;
			this.edFile.Text = fileRef.StringValue;

			if (this.fIsNew) {
				this.StoreTypesRefresh(true, MediaStoreType.mstReference);
			} else {
				string dummy = "";
				MediaStoreType gst = this.fBase.GetStoreType(fileRef, ref dummy);
				this.StoreTypesRefresh((gst == MediaStoreType.mstArchive), gst);
			}

			this.btnFileSelect.Enabled = this.fIsNew;
			this.cbStoreType.Enabled = this.fIsNew;

			this.fNotesList.DataList = this.fMediaRec.Notes.GetEnumerator();
			this.fSourcesList.DataList = this.fMediaRec.SourceCitations.GetEnumerator();
		}

		private void SetMediaRec(TGEDCOMMultimediaRecord value)
		{
			this.fMediaRec = value;
			try
			{
				this.ControlsRefresh();
				this.ActiveControl = this.edName;
			}
			catch (Exception ex)
			{
				this.fBase.Host.LogWrite("TfmMediaEdit.SetMediaRec(): " + ex.Message);
			}
		}

		private void btnAccept_Click(object sender, EventArgs e)
		{
			try
			{
				base.DialogResult = this.AcceptChanges() ? DialogResult.OK : DialogResult.None;
			}
			catch (Exception ex)
			{
				this.fBase.Host.LogWrite("TfmMediaEdit.Accept(): " + ex.Message);
				base.DialogResult = DialogResult.None;
			}
		}

		private void btnFileSelect_Click(object sender, EventArgs e)
		{
			if (this.OpenDialog1.ShowDialog() == DialogResult.OK)
			{
				string file = this.OpenDialog1.FileName;

				this.edFile.Text = file;
				TGEDCOMMultimediaFormat fileFmt = TGEDCOMFileReference.RecognizeFormat(file);

				FileInfo info = new FileInfo(file);
				double fileSize = (((double)info.Length / 1024) / 1024); // mb
			    bool canArc = (CheckFormatArchived(fileFmt) && fileSize <= 2);

                this.StoreTypesRefresh(canArc, MediaStoreType.mstReference);
				this.cbStoreType.Enabled = true;
			}
		}

        private static bool CheckFormatArchived(TGEDCOMMultimediaFormat format)
        {
            switch (format)
            {
                case TGEDCOMMultimediaFormat.mfWAV:
                case TGEDCOMMultimediaFormat.mfAVI:
                case TGEDCOMMultimediaFormat.mfMPG:
                    return false;
                default:
                    return true;
            }
        }

		private void btnView_Click(object sender, EventArgs e)
		{
			this.AcceptChanges();
			this.fBase.ShowMedia(this.fMediaRec, true);
		}

		private void edName_TextChanged(object sender, EventArgs e)
		{
			this.Text = LangMan.LS(LSID.LSID_RPMultimedia) + " \"" + this.edName.Text + "\"";
		}

		private void StoreTypesRefresh(bool allow_arc, MediaStoreType select)
		{
			this.cbStoreType.Items.Clear();
			this.cbStoreType.Items.Add(LangMan.LS(GKData.GKStoreTypes[(int)MediaStoreType.mstReference].Name));
			this.cbStoreType.Items.Add(LangMan.LS(GKData.GKStoreTypes[(int)MediaStoreType.mstStorage].Name));
			if (allow_arc) {
				this.cbStoreType.Items.Add(LangMan.LS(GKData.GKStoreTypes[(int)MediaStoreType.mstArchive].Name));
			}
			this.cbStoreType.SelectedIndex = (int)select;
		}

		public TfmMediaEdit(IBase aBase)
		{
			this.InitializeComponent();
			this.fBase = aBase;

			for (TGEDCOMMediaType mt = TGEDCOMMediaType.mtNone; mt <= TGEDCOMMediaType.mtLast; mt++)
			{
				this.cbMediaType.Items.Add(LangMan.LS(GKData.MediaTypes[(int)mt]));
			}

			this.fNotesList = new GKNotesSheet(this, this.SheetNotes);
			this.fSourcesList = new GKSourcesSheet(this, this.SheetSources);

			this.btnAccept.Text = LangMan.LS(LSID.LSID_DlgAccept);
			this.btnCancel.Text = LangMan.LS(LSID.LSID_DlgCancel);
			this.SheetCommon.Text = LangMan.LS(LSID.LSID_Common);
			this.SheetNotes.Text = LangMan.LS(LSID.LSID_RPNotes);
			this.SheetSources.Text = LangMan.LS(LSID.LSID_RPSources);
			this.Label1.Text = LangMan.LS(LSID.LSID_Title);
			this.Label2.Text = LangMan.LS(LSID.LSID_Type);
			this.Label4.Text = LangMan.LS(LSID.LSID_StoreType);
			this.Label3.Text = LangMan.LS(LSID.LSID_File);
			this.btnView.Text = LangMan.LS(LSID.LSID_View) + "...";
		}
	}
}
