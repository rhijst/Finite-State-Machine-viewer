using FSM.Visitors;

namespace FSM.Model;

public interface IElement
{
    void Accept(IVisitor visitor);
}
