namespace SurveyBasket.Api.Services
{
    public interface IPollService
    {
        IEnumerable<Poll> GetAll();
        Poll? Get(int id);
        Poll Add(Poll poll);
        bool Update(Poll poll,int id);
        bool Delete(int id);
    }
}
