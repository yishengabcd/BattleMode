using System;
public interface IAction
{
    //function synchronization():void;
    //function get finished() :Boolean;
    //function replace(action:IAction):Boolean;
    //function prepare():void;
    //function filter(action:IAction):Boolean;
    //function update():void;
    //function cancel():void;
    //function get priority() :int;
    //function dispose():void;
    //function getCode() : uint;
    //function ready($liftTime:int= 0) : Boolean;
    //function get type() :String;
    //function pause():void
    //function resume() :void;
    void Synchronization();
    bool Replace(IAction act);
    void Prepare();
    bool Filter(IAction act);
    void Update();
    void Cancel();
    void Dispose();
    bool Ready(int liftTime=0);
    void Pause();
    void Resume();
    bool Finished { get; }
    int Priority { get; }
    String Type { get; }
    int GetCode();

}
