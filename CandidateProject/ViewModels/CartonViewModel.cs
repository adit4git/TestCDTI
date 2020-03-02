namespace CandidateProject.ViewModels
{
    public class CartonViewModel
    {
        public int Id { get; set; }
        public string CartonNumber { get; set; }

        // AM: TODO BUG 1 - Display the # of pieces of equipment on the carton - DONE
        public bool AtCapacity { get; set; }
        //AM: TODO BUG3 = DONT ALLOW USER TO ADD MORE THAN 10 Equipment to a carton - DONE
        public int NoofEquipment { get; set; }
    }
}