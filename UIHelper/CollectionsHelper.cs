using BusinessLogicLayer;
using Entity;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace UIHelper
{
    public class CollectionsHelper
    {
        public static IEnumerable<SelectListItem> IEnumerableSupportedLanguages(SelectListItem itemToAddAtTop_ = null)
        {
            var supportedLanguagesList = new List<SelectListItem>();
            SelectListItem english_IN = new SelectListItem { Value = "en-IN", Text = "English (India)", Selected = true };
            SelectListItem hindi_IN = new SelectListItem { Value = "hi-IN", Text = "Hindi (India)" };
            supportedLanguagesList.Add(english_IN);
            supportedLanguagesList.Add(hindi_IN);
            return supportedLanguagesList.AsEnumerable<SelectListItem>();
        }

        public static IEnumerable<SelectListItem> IEnumerableOrdinalNumbers(int start_, int count_, SelectListItem itemToAddAtTop_ = null)
        {
            List<SelectListItem> result = new List<SelectListItem>();

            if (itemToAddAtTop_ != null)
                result.Add(itemToAddAtTop_);

            result.AddRange(
                from item in Enumerable.Range(start_, count_)
                select new SelectListItem { Text = item.ToString(), Value = item.ToString() });

            return result.AsEnumerable();
        }

        public static int CountStringOccurrences(string str, string pattern)
        {
            int count = 0;
            int i = 0;
            while ((i = str.IndexOf(pattern, i)) != -1)
            {
                i += pattern.Length;
                count++;
            }
            return count;
        }

        public static List<int> ListOfStudentRecords(int Document_ID_, int Package_ID_,int? Examination_ID)
        {
            var objENResult = new enResult { Document_ID = Document_ID_ };
            var objBLResult = new blResult(objENResult);
            List<enResult> listOfResult = new List<enResult>();

            listOfResult = objBLResult.ReadAll();

            var objENStudent = new enStudent();
            var objBLStudent = new blStudent(objENStudent);
            List<enStudent> listOfStudents = new List<enStudent>();

            listOfStudents = objBLStudent.ReadAll();

            var objENPackInvertory = new enPackageInventory { Package_ID = Package_ID_ };
            var objBLPackageInventory = new blPackageInventory(objENPackInvertory);
            List<enPackageInventory> listOfPackaageInventories = new List<enPackageInventory>();

            listOfPackaageInventories = objBLPackageInventory.ReadAll();

            int totalNoOfStudents = listOfStudents.FindAll(m => m.Group == Examination_ID.Value).Count;
            int testTakenByStudents = listOfResult.Count;
            //int paidStudents = listOfPackaageInventories.Count;
            int paidStudents = listOfStudents.FindAll(m => m.Group == Examination_ID.Value && m.StudentType == 1).Count; 
            List<int> result = new List<int>(); 
            result.Add(totalNoOfStudents);
            result.Add(paidStudents);
            result.Add(testTakenByStudents);
            return result;
        }

        public static List<int> TotalStudent(int? GroupID)
        {
            var objENStudent = new enStudent() { Group = GroupID };
            var objBLStudent = new blStudent(objENStudent);
            List<enStudent> listOfStudents = new List<enStudent>();
            listOfStudents = objBLStudent.ReadAll();

            int totalNoOfStudents = listOfStudents.Count;
            int paidStudents = listOfStudents.FindAll(m => m.StudentType == 1).Count;
            List<int> result = new List<int>();
            result.Add(totalNoOfStudents);
            result.Add(paidStudents);
            return result;
        }

        //public static int RankOfStudent(int result_ID, int Document_ID)
        //{
        //    var objENResult = new enResult { ID = result_ID };
        //    var objBLResult = new blResult(objENResult);
        //    List<enResult> listOfResults = new List<enResult>();

        //    listOfResults = objBLResult.ReadAllAndAggregate(typeof(enResultSummary));

        //    var resultSummary = listOfResults[0].listOfResultSummary.FindAll(m => m.Document_ID == Document_ID).OrderByDescending(m => m.Marks).ToList();

        //    var result = resultSummary.FindIndex(m => m.Result_ID == result_ID);

        //    return result + 1;
        //}

        public static int RankOfStudent(int result_ID, int Document_ID)
        {
            var objENResultSummary = new enResultSummary() { Document_ID = Document_ID };
            var objBLResultSummary = new blResultSummary(objENResultSummary);
            List<enResultSummary> listOfResultSummaries = new List<enResultSummary>();
            listOfResultSummaries = objBLResultSummary.ReadAll();
            var resultSummary = listOfResultSummaries.FindAll(m => m.Document_ID == Document_ID).OrderByDescending(m => m.Marks).ToList();
            var result = resultSummary.FindIndex(m => m.Result_ID == result_ID);
            return result + 1;
        }

        public static IEnumerable<SelectListItem> IEnumerableCriteria(SelectListItem itemToAddAtTop_ = null)
        {
            var statusList = new List<SelectListItem>();
            SelectListItem In_Active = new SelectListItem { Value = "0", Text = "Exact Match" };
            SelectListItem Active = new SelectListItem { Value = "1", Text = "By Range" };
            statusList.Add(Active);
            statusList.Add(In_Active);
            return statusList.AsEnumerable<SelectListItem>();
        }

        public static IEnumerable<SelectListItem> IEnumerableQuizAnswer(SelectListItem itemToAddAtTop_ = null)
        {
            var supportedLanguagesList = new List<SelectListItem>();
            SelectListItem OptionA = new SelectListItem { Value = "1", Text = "Option A" };
            SelectListItem OptionB = new SelectListItem { Value = "2", Text = "Option B" };
            SelectListItem OptionC = new SelectListItem { Value = "3", Text = "Option C" };
            SelectListItem OptionD = new SelectListItem { Value = "4", Text = "Option D" };
            supportedLanguagesList.Add(itemToAddAtTop_);
            supportedLanguagesList.Add(OptionA);
            supportedLanguagesList.Add(OptionB);
            supportedLanguagesList.Add(OptionC);
            supportedLanguagesList.Add(OptionD);
            return supportedLanguagesList.AsEnumerable<SelectListItem>();
        }

        public static IEnumerable<SelectListItem> IEnumerableStudentStatus(SelectListItem itemToAddAtTop_ = null)
        {
            var supportedLanguagesList = new List<SelectListItem>();
            SelectListItem OptionA = new SelectListItem { Value = "1", Text = "Internal" };
            SelectListItem OptionB = new SelectListItem { Value = "2", Text = "External" };
            supportedLanguagesList.Add(itemToAddAtTop_);
            supportedLanguagesList.Add(OptionA);
            supportedLanguagesList.Add(OptionB);
            return supportedLanguagesList.AsEnumerable<SelectListItem>();
        }

        public static IEnumerable<SelectListItem> IEnumerableModeOfPayment(SelectListItem itemToAddAtTop_ = null)
        {
            var supportedLanguagesList = new List<SelectListItem>();
            SelectListItem Cash = new SelectListItem { Value = "1", Text = "Cash" };
            SelectListItem Paytm = new SelectListItem { Value = "2", Text = "Paytm" };
            SelectListItem PhonePay = new SelectListItem { Value = "3", Text = "PhonePay" };
            SelectListItem GooglePay = new SelectListItem { Value = "4", Text = "GooglePay" };
            SelectListItem FromBank = new SelectListItem { Value = "5", Text = "FromBank" };
            supportedLanguagesList.Add(itemToAddAtTop_);
            supportedLanguagesList.Add(Cash);
            supportedLanguagesList.Add(Paytm);
            supportedLanguagesList.Add(PhonePay);
            supportedLanguagesList.Add(GooglePay);
            supportedLanguagesList.Add(FromBank);
            return supportedLanguagesList.AsEnumerable<SelectListItem>();
        }
    }
}