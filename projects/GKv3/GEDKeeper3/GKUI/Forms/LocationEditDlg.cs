﻿/*
 *  "GEDKeeper", the personal genealogical database editor.
 *  Copyright (C) 2009-2017 by Sergey V. Zhdanovskih.
 *
 *  This file is part of "GEDKeeper".
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using BSLib;
using Eto.Forms;
using GKCommon;
using GKCommon.GEDCOM;
using GKCore;
using GKCore.Geocoding;
using GKCore.Interfaces;
using GKCore.Lists;
using GKCore.Maps;
using GKCore.Types;
using GKCore.UIContracts;
using GKUI.Components;

namespace GKUI.Forms
{
    /// <summary>
    /// 
    /// </summary>
    public sealed partial class LocationEditDlg : EditorDialog, ILocationEditDlg
    {
        private readonly GKMapBrowser fMapBrowser;
        private readonly GKSheetList fMediaList;
        private readonly GKSheetList fNotesList;

        private GEDCOMLocationRecord fLocationRecord;

        public GEDCOMLocationRecord LocationRecord
        {
            get {
                return fLocationRecord;
            }
            set {
                SetLocationRecord(value);
            }
        }

        public LocationEditDlg()
        {
            InitializeComponent();

            fMapBrowser = new GKMapBrowser();
            fMapBrowser.InitMap();
            fMapBrowser.ShowLines = false;
            panMap.Content = fMapBrowser;

            fNotesList = new GKSheetList(pageNotes);
            fMediaList = new GKSheetList(pageMultimedia);

            // SetLang()
            Title = LangMan.LS(LSID.LSID_Location);
            btnAccept.Text = LangMan.LS(LSID.LSID_DlgAccept);
            btnCancel.Text = LangMan.LS(LSID.LSID_DlgCancel);
            pageCommon.Text = LangMan.LS(LSID.LSID_Common);
            pageNotes.Text = LangMan.LS(LSID.LSID_RPNotes);
            pageMultimedia.Text = LangMan.LS(LSID.LSID_RPMultimedia);
            lblName.Text = LangMan.LS(LSID.LSID_Title);
            lblLatitude.Text = LangMan.LS(LSID.LSID_Latitude);
            lblLongitude.Text = LangMan.LS(LSID.LSID_Longitude);
            ListGeoCoords.SetColumnCaption(0, LangMan.LS(LSID.LSID_Title));
            ListGeoCoords.SetColumnCaption(1, LangMan.LS(LSID.LSID_Latitude));
            ListGeoCoords.SetColumnCaption(2, LangMan.LS(LSID.LSID_Longitude));
            btnShowOnMap.Text = LangMan.LS(LSID.LSID_Show);
            grpSearch.Text = LangMan.LS(LSID.LSID_SearchCoords);
            btnSearch.Text = LangMan.LS(LSID.LSID_Search);
            btnSelect.Text = LangMan.LS(LSID.LSID_SelectCoords);
            btnSelectName.Text = LangMan.LS(LSID.LSID_SelectName);
            btnShowOnMap.ToolTip = LangMan.LS(LSID.LSID_ShowOnMapTip);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) {
            }
            base.Dispose(disposing);
        }

        private void SetLocationRecord(GEDCOMLocationRecord value)
        {
            fLocationRecord = value;
            txtName.Text = fLocationRecord.LocationName;
            txtLatitude.Text = PlacesLoader.CoordToStr(fLocationRecord.Map.Lati);
            txtLongitude.Text = PlacesLoader.CoordToStr(fLocationRecord.Map.Long);

            fNotesList.ListModel.DataOwner = fLocationRecord;
            fMediaList.ListModel.DataOwner = fLocationRecord;

            txtName.Focus();
        }

        private void EditName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Keys.Down && e.Control) {
                txtName.Text = txtName.Text.ToLower();
            }
        }

        private void btnAccept_Click(object sender, EventArgs e)
        {
            try {
                fLocationRecord.LocationName = txtName.Text;
                fLocationRecord.Map.Lati = ConvertHelper.ParseFloat(txtLatitude.Text, 0.0);
                fLocationRecord.Map.Long = ConvertHelper.ParseFloat(txtLongitude.Text, 0.0);

                CommitChanges();

                fBase.NotifyRecord(fLocationRecord, RecordAction.raEdit);

                DialogResult = DialogResult.Ok;
            } catch (Exception ex) {
                Logger.LogWrite("LocationEditDlg.btnAccept_Click(): " + ex.Message);
                DialogResult = DialogResult.None;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            try {
                RollbackChanges();
                CancelClickHandler(sender, e);
            } catch (Exception ex) {
                Logger.LogWrite("LocationEditDlg.btnCancel_Click(): " + ex.Message);
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string location = txtName.Text.Trim();
            if (string.IsNullOrEmpty(location)) {
                return;
            }

            ListGeoCoords.BeginUpdate();
            fMapBrowser.BeginUpdate();
            try {
                IList<GeoPoint> searchPoints = new List<GeoPoint>();

                AppHost.Instance.RequestGeoCoords(location, searchPoints);
                ListGeoCoords.ClearItems();
                fMapBrowser.ClearPoints();

                int num = searchPoints.Count;
                for (int i = 0; i < num; i++) {
                    GeoPoint pt = searchPoints[i];

                    ListGeoCoords.AddItem(pt, pt.Hint,
                        PlacesLoader.CoordToStr(pt.Latitude),
                        PlacesLoader.CoordToStr(pt.Longitude));

                    fMapBrowser.AddPoint(pt.Latitude, pt.Longitude, pt.Hint);

                    if (i == 0) {
                        fMapBrowser.SetCenter(pt.Latitude, pt.Longitude, -1);
                    }
                }

                //fMapBrowser.ZoomToBounds();
            } finally {
                fMapBrowser.EndUpdate();
                ListGeoCoords.EndUpdate();
            }
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            GKListItem item = ListGeoCoords.GetSelectedItem();
            if (item != null) {
                GeoPoint pt = (GeoPoint)item.Data;
                txtLatitude.Text = PlacesLoader.CoordToStr(pt.Latitude);
                txtLongitude.Text = PlacesLoader.CoordToStr(pt.Longitude);
            }
        }

        private void btnSelectName_Click(object sender, EventArgs e)
        {
            GKListItem item = ListGeoCoords.GetSelectedItem();
            if (item != null) {
                GeoPoint pt = (GeoPoint)item.Data;
                txtName.Text = pt.Hint;
            }
        }

        private void ListGeoCoords_Click(object sender, EventArgs e)
        {
            GKListItem item = ListGeoCoords.GetSelectedItem();
            if (item != null) {
                GeoPoint pt = (GeoPoint)item.Data;
                fMapBrowser.SetCenter(pt.Latitude, pt.Longitude, -1);
            }
        }

        private void EditName_TextChanged(object sender, EventArgs e)
        {
            Title = string.Format("{0} \"{1}\"", LangMan.LS(LSID.LSID_Location), txtName.Text);
        }

        private void btnShowOnMap_Click(object sender, EventArgs e)
        {
            if (txtLatitude.Text != "" && txtLongitude.Text != "") {
                fMapBrowser.SetCenter(ConvertHelper.ParseFloat(txtLatitude.Text, 0), ConvertHelper.ParseFloat(txtLongitude.Text, 0), -1);
            }
        }

        public override void InitDialog(IBaseWindow baseWin)
        {
            base.InitDialog(baseWin);

            fNotesList.ListModel = new NoteLinksListModel(fBase, fLocalUndoman);
            fMediaList.ListModel = new MediaLinksListModel(fBase, fLocalUndoman);
        }
    }
}
