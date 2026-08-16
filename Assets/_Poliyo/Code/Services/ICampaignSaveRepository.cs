namespace Poliyo.Application
{
public interface ICampaignSaveRepository
{
    void Save(string slotId, CampaignSaveData saveData);
    CampaignSaveData Load(string slotId);
    bool Exists(string slotId);
}

}
