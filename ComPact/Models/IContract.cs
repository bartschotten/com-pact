namespace ComPact.Models
{
    internal interface IContract
    {
        Pacticipant Consumer { get; set; }
        Pacticipant Provider { get; set; }
        void SetEmptyValuesToNull();
    }
}
