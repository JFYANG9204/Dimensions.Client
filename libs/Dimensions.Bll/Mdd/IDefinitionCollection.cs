
namespace Dimensions.Bll.Mdd
{
    public interface IDefinitionCollection
    {
        IDefinition this[int index] { get; }
        int Count { get; }
        void Add(IDefinition definition);
        bool Exists(string varName);
    }
}
