﻿using System.Drawing;
using System.IO;

using GKCommon;
using GKCommon.GEDCOM;
using GKCommon.GEDCOM.Enums;
using GKCommon.Graph;
using GKCore.Types;

namespace GKCore.Interfaces
{
	public interface IBaseContext
	{
		GEDCOMTree Tree { get; }

		void Clear();
		void FileLoad(string fileName, string password = null);
		void FileSave(string fileName, string password = null);

		// Data search
		GEDCOMSourceRecord FindSource(string sourceName);
		void GetSourcesList(StringList sources);

		// Data manipulation
		GEDCOMCustomEvent CreateEventEx(GEDCOMRecordWithEvents aRec, string evSign, string evDate, string evPlace);
		GEDCOMIndividualRecord CreatePersonEx(string iName, string iPatronymic, string iSurname, GEDCOMSex iSex, bool birthEvent);

		// Individual utils
		bool IsChildless(GEDCOMIndividualRecord iRec);
		int FindBirthYear(GEDCOMIndividualRecord iRec);
		int FindDeathYear(GEDCOMIndividualRecord iRec);
		
		// Patriarchs search
		void GetPatriarchsList(ExtList<PatriarchObj> patList, int gensMin, bool datesCheck);
		void GetPatriarchsLinks(ExtList<PatriarchObj> patList, int gensMin, bool datesCheck, bool loneSuppress);
		TGraph GetPatriarchsGraph(int gensMin, bool datesCheck, bool loneSuppress = true);
		
		// Multimedia support
		bool CheckBasePath();
		MediaStoreType GetStoreType(GEDCOMFileReference fileReference, ref string fileName);
		void MediaLoad(GEDCOMFileReference fileReference, out Stream stream, bool throwException);
		void MediaLoad(GEDCOMFileReference fileReference, ref string fileName);
		bool MediaSave(GEDCOMFileReference fileReference, string fileName, MediaStoreType storeType);
		Bitmap BitmapLoad(GEDCOMFileReference fileReference, int thumbWidth, int thumbHeight, bool throwException);
		Bitmap GetPrimaryBitmap(GEDCOMIndividualRecord iRec, int thumbWidth, int thumbHeight, bool throwException);
	}
}
