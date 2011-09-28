﻿using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

using GedCom551;
using GKCore.Sys;

namespace GKCore
{
	public class TPedigree : THTMLExporter
	{
		private class TPersonObj
		{
			public TPedigree.TPersonObj Parent;
			public string Id;
			public TGEDCOMIndividualRecord iRec;
			public int Level;
			public string BirthDate;
			public string Sources;
			public int FamilyOrder;
			public int ChildIdx;

			public string GetOrderStr()
			{
				char order = (char)this.FamilyOrder;
				string Result;
				if (this.Parent == null)
				{
					Result = new string(order, 1);
				}
				else
				{
					Result = this.Parent.GetOrderStr() + order;
				}
				return Result;
			}

			public void Free()
			{
				TObjectHelper.Free(this);
			}
		}

		private class TEventObj
		{
			public TGEDCOMCustomEvent Event;
			public TGEDCOMIndividualRecord iRec;

			public TEventObj(TGEDCOMCustomEvent aEvent, TGEDCOMIndividualRecord aRec)
			{
				this.Event = aEvent;
				this.iRec = aRec;
			}

			public DateTime GetDate()
			{
				DateTime Result;
				if (this.Event != null)
				{
					Result = TGenEngine.GEDCOMDateToDate(this.Event.Detail.Date.Value);
				}
				else
				{
					Result = new DateTime(0);
				}
				return Result;
			}

			public void Free()
			{
				TObjectHelper.Free(this);
			}

		}

		public enum TPedigreeKind : byte
		{
			pk_dAboville,
			pk_Konovalov
		}

		private TGEDCOMIndividualRecord FAncestor;
		private TPedigreeKind FKind;
		private TObjectList FPersonList;
		private TGenEngine.TShieldState FShieldState;
		private TStringList FSourceList;


		public TGEDCOMIndividualRecord Ancestor
		{
			get { return this.FAncestor; }
			set { this.FAncestor = value; }
		}

		public TPedigree.TPedigreeKind Kind
		{
			get { return this.FKind; }
			set { this.FKind = value; }
		}

		public TGenEngine.TShieldState ShieldState
		{
			get { return this.FShieldState; }
			set { this.FShieldState = value; }
		}

		private TPersonObj FindPerson(TGEDCOMIndividualRecord iRec)
		{
			TPedigree.TPersonObj res = null;

			int num = this.FPersonList.Count - 1;
			for (int i = 0; i <= num; i++)
			{
				if ((this.FPersonList[i] as TPersonObj).iRec == iRec)
				{
					res = (this.FPersonList[i] as TPersonObj);
					break;
				}
			}

			return res;
		}

		private void WritePerson(StreamWriter aStream, TGEDCOMTree aTree, TPedigree.TPersonObj aPerson)
		{
			aStream.WriteLine("<li>");
			aStream.WriteLine(string.Concat(new string[]
			{
				"<b>", 
				this.GetIdStr(aPerson), 
				". ", 
				TGenEngine.GetNameStr(aPerson.iRec, true, false), 
				"</b>", 
				this.GetPedigreeLifeStr(aPerson.iRec)
			}));
			if (this.FOptions.PedigreeOptions.IncludeSources)
			{
				aStream.WriteLine("&nbsp;<sup>" + aPerson.Sources + "</sup>");
			}
			TPedigreeOptions.TPedigreeFormat format = this.FOptions.PedigreeOptions.Format;
			if (format != TPedigreeOptions.TPedigreeFormat.pfExcess)
			{
				if (format == TPedigreeOptions.TPedigreeFormat.pfCompact)
				{
					this.WriteCompactFmt(aStream, aPerson);
				}
			}
			else
			{
				this.WriteExcessFmt(aStream, aPerson);
			}
			aStream.WriteLine("</li><br>");
		}

		private string idLink(TPedigree.TPersonObj aObj)
		{
			string res = "";
			if (aObj != null)
			{
				res = " [<a href=\"#" + aObj.Id + "\">" + aObj.Id + "</a>]";
			}
			return res;
		}

		private string GetIdStr(TPedigree.TPersonObj aPerson)
		{
			string Result = "<a name=\"" + aPerson.Id + "\">" + aPerson.Id + "</a>";

			if (this.FKind == TPedigree.TPedigreeKind.pk_Konovalov && aPerson.Parent != null)
			{
				TGEDCOMFamilyRecord family = aPerson.iRec.ChildToFamilyLinks[0].Family;
				string sp_str = "";
				int idx = aPerson.Parent.iRec.IndexOfSpouse(family);
				if (aPerson.Parent.iRec.SpouseToFamilyLinks.Count > 1)
				{
					sp_str = "/" + (idx + 1).ToString();
				}
				Result += sp_str;
			}
			return Result;
		}

		private void WriteExcessFmt(StreamWriter aStream, TPedigree.TPersonObj aPerson)
		{
			aStream.WriteLine("<br>" + GKL.LSList[87] + ": " + TGenEngine.SexStr(aPerson.iRec.Sex));
			string st = TGenEngine.GetLifeExpectancy(aPerson.iRec);

			if (st != "?" && st != "")
			{
				aStream.WriteLine("<br>" + GKL.LSList[306] + ": " + st);
			}

			if (aPerson.iRec.ChildToFamilyLinks.Count != 0)
			{
				TGEDCOMFamilyRecord family = aPerson.iRec.ChildToFamilyLinks[0].Family;
				TGEDCOMIndividualRecord irec = family.Husband.Value as TGEDCOMIndividualRecord;
				if (irec != null)
				{
					aStream.WriteLine(string.Concat(new string[]
					{
						"<br>", 
						GKL.LSList[150], 
						": ", 
						TGenEngine.GetNameStr(irec, true, false), 
						this.idLink(this.FindPerson(irec))
					}));
				}
				irec = (family.Wife.Value as TGEDCOMIndividualRecord);
				if (irec != null)
				{
					aStream.WriteLine(string.Concat(new string[]
					{
						"<br>", 
						GKL.LSList[151], 
						": ", 
						TGenEngine.GetNameStr(irec, true, false), 
						this.idLink(this.FindPerson(irec))
					}));
				}
			}

			TObjectList ev_list = new TObjectList(true);
			try
			{
				int i;
				if (aPerson.iRec.IndividualEvents.Count > 0)
				{
					aStream.WriteLine("<p>" + GKL.LSList[83] + ": <ul>");

					int num = aPerson.iRec.IndividualEvents.Count - 1;
					for (i = 0; i <= num; i++)
					{
						TGEDCOMCustomEvent @event = aPerson.iRec.IndividualEvents[i];
						if (!(@event is TGEDCOMIndividualAttribute) || (@event is TGEDCOMIndividualAttribute && this.FOptions.PedigreeOptions.IncludeAttributes))
						{
							ev_list.Add(new TPedigree.TEventObj(@event, aPerson.iRec));
						}
					}
					this.WriteEventList(aStream, aPerson, ev_list);
					aStream.WriteLine("</ul></p>");
				}

				int num2 = aPerson.iRec.SpouseToFamilyLinks.Count - 1;
				for (i = 0; i <= num2; i++)
				{
					TGEDCOMFamilyRecord family = aPerson.iRec.SpouseToFamilyLinks[i].Family;
					if (TGenEngine.IsRecordAccess(family.Restriction, this.FShieldState))
					{
						TGEDCOMPointer sp;
						string unk;
						if (aPerson.iRec.Sex == TGEDCOMSex.svMale)
						{
							sp = family.Wife;
							st = GKL.LSList[116] + ": ";
							unk = GKL.LSList[63];
						}
						else
						{
							sp = family.Husband;
							st = GKL.LSList[115] + ": ";
							unk = GKL.LSList[64];
						}
						TGEDCOMIndividualRecord irec = sp.Value as TGEDCOMIndividualRecord;
						if (irec != null)
						{
							aStream.WriteLine(string.Concat(new string[]
							{
								"<p>", 
								st, 
								TGenEngine.GetNameStr(irec, true, false), 
								this.GetPedigreeLifeStr(irec), 
								this.idLink(this.FindPerson(irec))
							}));
						}
						else
						{
							aStream.WriteLine("<p>" + st + unk);
						}
						aStream.WriteLine("<ul>");
						ev_list.Clear();

						int num3 = family.Childrens.Count - 1;
						for (int j = 0; j <= num3; j++)
						{
							irec = (family.Childrens[j].Value as TGEDCOMIndividualRecord);
							ev_list.Add(new TPedigree.TEventObj(TGenEngine.GetIndividualEvent(irec, "BIRT"), irec));
						}
						this.WriteEventList(aStream, aPerson, ev_list);
						aStream.WriteLine("</ul></p>");
					}
				}
			}
			finally
			{
				ev_list.Free();
			}

			if (this.FOptions.PedigreeOptions.IncludeNotes && aPerson.iRec.Notes.Count != 0)
			{
				aStream.WriteLine("<p>" + GKL.LSList[54] + ":<ul>");

				int num4 = aPerson.iRec.Notes.Count - 1;
				for (int i = 0; i <= num4; i++)
				{
					TGEDCOMNotes note = aPerson.iRec.Notes[i];
					aStream.WriteLine("<li>" + SysUtils.ConStrings(note.Notes) + "</li>");
				}
				aStream.WriteLine("</ul></p>");
			}
		}

		private void WriteCompactFmt(StreamWriter aStream, TPedigree.TPersonObj aPerson)
		{
			if (this.FOptions.PedigreeOptions.IncludeNotes && aPerson.iRec.Notes.Count != 0)
			{
				int num = aPerson.iRec.Notes.Count - 1;
				for (int i = 0; i <= num; i++)
				{
					TGEDCOMNotes note = aPerson.iRec.Notes[i];
					aStream.WriteLine("<p style=\"margin-top:2pt; margin-bottom:2pt\">" + SysUtils.ConStrings(note.Notes) + "</p>");
				}
			}
			try
			{
				bool sp_index = aPerson.iRec.SpouseToFamilyLinks.Count > 1;

				int num2 = aPerson.iRec.SpouseToFamilyLinks.Count - 1;
				for (int i = 0; i <= num2; i++)
				{
					TGEDCOMFamilyRecord family = aPerson.iRec.SpouseToFamilyLinks[i].Family;
					if (TGenEngine.IsRecordAccess(family.Restriction, this.FShieldState))
					{
						TGEDCOMPointer sp;
						string st;
						string unk;
						if (aPerson.iRec.Sex == TGEDCOMSex.svMale)
						{
							sp = family.Wife;
							st = "Ж";
							unk = GKL.LSList[63];
						}
						else
						{
							sp = family.Husband;
							st = "М";
							unk = GKL.LSList[64];
						}
						if (sp_index)
						{
							st += (i + 1).ToString();
						}
						st += " - ";
						TGEDCOMIndividualRecord irec = sp.Value as TGEDCOMIndividualRecord;
						if (irec != null)
						{
							st = st + TGenEngine.GetNameStr(irec, true, false) + this.GetPedigreeLifeStr(irec) + this.idLink(this.FindPerson(irec));
						}
						else
						{
							st += unk;
						}
						aStream.WriteLine("<p style=\"margin-top:2pt; margin-bottom:2pt\">" + st + "</p>");
					}
				}
			}
			finally
			{
			}
		}

		private string GetPedigreeLifeStr(TGEDCOMIndividualRecord iRec)
		{
			string res_str = "";
			TPedigreeOptions.TPedigreeFormat format = this.FOptions.PedigreeOptions.Format;
			if (format != TPedigreeOptions.TPedigreeFormat.pfExcess)
			{
				if (format == TPedigreeOptions.TPedigreeFormat.pfCompact)
				{
					string ds = TGenEngine.GetBirthDate(iRec, TGenEngine.TDateFormat.dfDD_MM_YYYY, true);
					string ps = TGenEngine.GetBirthPlace(iRec);
					if (ps != "")
					{
						if (ds != "")
						{
							ds += ", ";
						}
						ds += ps;
					}
					if (ds != "")
					{
						ds = "*" + ds;
					}
					res_str += ds;
					ds = TGenEngine.GetDeathDate(iRec, TGenEngine.TDateFormat.dfDD_MM_YYYY, true);
					ps = TGenEngine.GetDeathPlace(iRec);
					if (ps != "")
					{
						if (ds != "")
						{
							ds += ", ";
						}
						ds += ps;
					}
					if (ds != "")
					{
						ds = "+" + ds;
					}
					if (ds != "")
					{
						res_str = res_str + " " + ds;
					}
				}
			}
			else
			{
				string ds = TGenEngine.GetBirthDate(iRec, TGenEngine.TDateFormat.dfDD_MM_YYYY, true);
				if (ds == "")
				{
					ds = "?";
				}
				res_str += ds;
				ds = TGenEngine.GetDeathDate(iRec, TGenEngine.TDateFormat.dfDD_MM_YYYY, true);
				if (ds == "")
				{
					TGEDCOMCustomEvent ev = TGenEngine.GetIndividualEvent(iRec, "DEAT");
					if (ev != null)
					{
						ds = "?";
					}
				}
				if (ds != "")
				{
					res_str = res_str + " - " + ds;
				}
			}
			string Result;
			if (res_str == "" || res_str == " ")
			{
				Result = "";
			}
			else
			{
				Result = " (" + res_str + ")";
			}
			return Result;
		}

		private void WriteEventList(StreamWriter aStream, TPedigree.TPersonObj aPerson, TObjectList ev_list)
		{
			int num = ev_list.Count - 1;
			for (int i = 0; i <= num; i++)
			{
				int num2 = ev_list.Count - 1;
				for (int j = i + 1; j <= num2; j++)
				{
					if ((ev_list[i] as TPedigree.TEventObj).GetDate() > (ev_list[j] as TPedigree.TEventObj).GetDate())
					{
						ev_list.Exchange(i, j);
					}
				}
			}

			int num3 = ev_list.Count - 1;
			for (int i = 0; i <= num3; i++)
			{
				TGEDCOMCustomEvent @event = (ev_list[i] as TPedigree.TEventObj).Event;
				if (@event != null && object.Equals((ev_list[i] as TPedigree.TEventObj).iRec, aPerson.iRec))
				{
					if (@event.Name == "BIRT")
					{
						ev_list.Exchange(i, 0);
					}
					else
					{
						if (@event.Name == "DEAT")
						{
							ev_list.Exchange(i, ev_list.Count - 1);
						}
					}
				}
			}

			int num4 = ev_list.Count - 1;
			for (int i = 0; i <= num4; i++)
			{
				TPedigree.TEventObj evObj = ev_list[i] as TPedigree.TEventObj;
				TGEDCOMCustomEvent @event = evObj.Event;
				if (object.Equals(evObj.iRec, aPerson.iRec))
				{
					int ev = TGenEngine.GetPersonEventIndex(@event.Name);
					string st;
					if (ev == 0)
					{
						st = @event.Detail.Classification;
					}
					else
					{
						if (ev > 0)
						{
							st = GKL.LSList[(int)TGenEngine.PersonEvents[ev].Name - 1];
						}
						else
						{
							st = @event.Name;
						}
					}
					string dt = TGenEngine.GEDCOMCustomDateToStr(@event.Detail.Date.Value, TGenEngine.TDateFormat.dfDD_MM_YYYY, false);
					aStream.WriteLine("<li>" + dt + ": " + st + ".");
					if (@event.Detail.Place.StringValue != "")
					{
						aStream.WriteLine(string.Concat(new string[]
						{ " ", GKL.LSList[204], ": ", @event.Detail.Place.StringValue, "</li>" }));
					}
				}
				else
				{
					string dt;
					if (@event == null)
					{
						dt = "?";
					}
					else
					{
						dt = TGenEngine.GEDCOMCustomDateToStr(@event.Detail.Date.Value, TGenEngine.TDateFormat.dfDD_MM_YYYY, false);
					}
					string st;
					if (evObj.iRec.Sex == TGEDCOMSex.svMale)
					{
						st = ": Родился ";
					}
					else
					{
						st = ": Родилась ";
					}
					aStream.WriteLine(string.Concat(new string[]
					{ "<li>", dt, st, TGenEngine.GetNameStr(evObj.iRec, true, false), this.idLink(this.FindPerson(evObj.iRec)), "</li>" }));
				}
			}
		}

		public override void Generate()
		{
			if (this.FAncestor == null)
			{
				SysUtils.ShowError(GKL.LSList[209]);
			}
			else
			{
				string title = GKL.LSList[484] + ": " + TGenEngine.GetNameStr(this.FAncestor, true, false);
				SysUtils.CreateDir(this.FPath);
				StreamWriter fs_index = new StreamWriter(this.FPath + "pedigree.htm", false, Encoding.GetEncoding(1251));
				base.WriteHeader(fs_index, title);
				fs_index.WriteLine("<h2>" + title + "</h2>");
				this.FPersonList = new TObjectList(true);
				this.FSourceList = new TStringList();
				try
				{
					TPedigree._Generate_Step(this, null, this.FAncestor, 1, 1);
					TPedigree._Generate_ReIndex(this);
					int cur_level = 0;

					int num = this.FPersonList.Count - 1;
					for (int i = 0; i <= num; i++)
					{
						TPedigree.TPersonObj pObj = this.FPersonList[i] as TPedigree.TPersonObj;
						if (cur_level != pObj.Level)
						{
							if (cur_level > 0)
							{
								fs_index.WriteLine("</ul>");
							}
							cur_level = pObj.Level;
							fs_index.WriteLine(string.Concat(new string[]
							{
								"<h3>", 
								GKL.LSList[399], 
								" ", 
								SysUtils.GetRome(cur_level), 
								"</h3><ul>"
							}));
						}
						this.WritePerson(fs_index, this.FTree, pObj);
					}
					fs_index.WriteLine("</ul>");
					if (this.FSourceList.Count > 0)
					{
						fs_index.WriteLine("<h3>" + GKL.LSList[56] + "</h3>");

						int num2 = this.FSourceList.Count - 1;
						for (int j = 0; j <= num2; j++)
						{
							string sn = (j + 1).ToString();
							fs_index.WriteLine(string.Concat(new string[]
							{ "<p><sup><a name=\"src", sn, "\">", sn, "</a></sup>&nbsp;" }));
							fs_index.WriteLine(this.FSourceList[j] + "</p>");
						}
					}
				}
				finally
				{
					this.FSourceList.Free();
					this.FPersonList.Free();
				}
				base.WriteFooter(fs_index);
				TObjectHelper.Free(fs_index);
				SysUtils.LoadExtFile(this.FPath + "pedigree.htm");
			}
		}

		public TPedigree(TGenEngine aEngine, string aPath) : base(aEngine, aPath)
		{
		}

		private static void _Generate_Step([In] TPedigree Self, TPedigree.TPersonObj aParent, TGEDCOMIndividualRecord iRec, int aLevel, int aFamilyOrder)
		{
			if (iRec != null)
			{
				TPedigree.TPersonObj res = new TPedigree.TPersonObj();
				res.Parent = aParent;
				res.iRec = iRec;
				res.Level = aLevel;
				res.ChildIdx = 0;
				res.BirthDate = TGenEngine.GetBirthDate(iRec, TGenEngine.TDateFormat.dfYYYY_MM_DD, true);
				res.FamilyOrder = aFamilyOrder;
				Self.FPersonList.Add(res);
				string i_sources = "";
				int j;
				if (Self.FOptions.PedigreeOptions.IncludeSources)
				{
					int num = iRec.SourceCitations.Count - 1;
					for (int i = 0; i <= num; i++)
					{
						TGEDCOMSourceCitation cit = iRec.SourceCitations[i];
						TGEDCOMSourceRecord sourceRec = cit.Value as TGEDCOMSourceRecord;
						if (sourceRec != null)
						{
							string src_name = SysUtils.ConStrings(sourceRec.Title);
							if (src_name == "")
							{
								src_name = sourceRec.FiledByEntry;
							}
							j = Self.FSourceList.IndexOf(src_name);
							if (j < 0)
							{
								j = Self.FSourceList.Add(src_name);
							}
							if (i_sources != "")
							{
								i_sources += ",";
							}
							string sn = (j + 1).ToString();
							i_sources = i_sources + "<a href=\"#src" + sn + "\">" + sn + "</a>";
						}
					}
				}
				res.Sources = i_sources;

				int num2 = iRec.SpouseToFamilyLinks.Count - 1;
				for (j = 0; j <= num2; j++)
				{
					TGEDCOMFamilyRecord family = iRec.SpouseToFamilyLinks[j].Family;
					if (TGenEngine.IsRecordAccess(family.Restriction, Self.FShieldState))
					{
						family.SortChilds();

						int num3 = family.Childrens.Count - 1;
						for (int i = 0; i <= num3; i++)
						{
							TGEDCOMIndividualRecord child = family.Childrens[i].Value as TGEDCOMIndividualRecord;
							TPedigree._Generate_Step(Self, res, child, aLevel + 1, i + 1);
						}
					}
				}
			}
		}

		private static void _Generate_ReIndex([In] TPedigree Self)
		{
			int num = Self.FPersonList.Count - 1;
			for (int i = 0; i <= num; i++)
			{
				int num2 = Self.FPersonList.Count - 1;
				for (int j = i + 1; j <= num2; j++)
				{
					TPedigree.TPersonObj obj = Self.FPersonList[i] as TPedigree.TPersonObj;
					TPedigree.TPersonObj obj2 = Self.FPersonList[j] as TPedigree.TPersonObj;
					string i_str = (char)obj.Level + obj.GetOrderStr();
					string k_str = (char)obj2.Level + obj2.GetOrderStr();
					if (string.Compare(i_str, k_str, false) > 0)
					{
						Self.FPersonList.Exchange(i, j);
					}
				}
			}

			int num3 = Self.FPersonList.Count - 1;
			for (int i = 0; i <= num3; i++)
			{
				TPedigree.TPersonObj obj = Self.FPersonList[i] as TPedigree.TPersonObj;
				TPedigree.TPedigreeKind fKind = Self.FKind;
				if (fKind != TPedigree.TPedigreeKind.pk_dAboville)
				{
					if (fKind == TPedigree.TPedigreeKind.pk_Konovalov)
					{
						obj.Id = (i + 1).ToString();
						if (obj.Parent != null)
						{
							string pid = obj.Parent.Id;
							int p = SysUtils.Pos("-", pid);
							if (p > 0)
							{
								pid = SysUtils.WStrCopy(pid, 1, p - 1);
							}
							obj.Id = obj.Id + "-" + pid;
						}
					}
				}
				else
				{
					if (obj.Parent == null)
					{
						obj.Id = "1";
					}
					else
					{
						obj.Parent.ChildIdx++;
						obj.Id = obj.Parent.Id + "." + obj.Parent.ChildIdx.ToString();
					}
				}
			}
		}
	}
}
