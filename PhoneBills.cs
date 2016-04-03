using System;
using System.Windows.Forms;
using GTA;
using GTA.Native;

// PHONE BILLS Script for GTA V - Created by EnforcerZhukov (http://bit.do/zhukovyt)

public class PhoneBills : Script {
    private ScriptSettings config;
    //variables to be dynamically changed while playing
        bool usingphone; //are you actually calling?
        bool usedphone; //have you called recently but not now?
        bool startcalltimegenerated; //have you started calling?
        int calltime; //final call time, auto-calculated
        int startcalltime; //when the call started?

    //variables to be statically customized by the player
        bool freeshortcalls; //will short calls be free of charge?
        int freeshortcallsmaxtime; //max. time the call will be considered a "short call"
        int mincallcost; //calls will always cost this at least
        int maxcallcost; //calls can't cost more than this
        int callcost; //call cost per second
        int feecost; //call cost tax -each call can have a extra fee
        bool feecostonshortcalls; //apply fees on short calls?
        bool basicdebug; //basic call debug-when calling ends
        bool fulldebug; //advanced call debug...

    public PhoneBills() {
        config = ScriptSettings.Load("scripts\\PhoneBills.ini"); //get the INI config file location
        //grab the customizable options from the INI config file
        callcost = config.GetValue<int>("COST", "CallCost", 1);
        freeshortcalls = config.GetValue<bool>("COST", "FreeShortCalls", false);
        freeshortcallsmaxtime = config.GetValue<int>("COST", "FreeShortCallsMaxTime", 10);
        mincallcost = config.GetValue<int>("COST", "MinCallCost", 1);
        maxcallcost = config.GetValue<int>("COST", "MaxCallCost", 420);
        feecost = config.GetValue<int>("COST", "CallFee", 0);
        feecostonshortcalls = config.GetValue<bool>("COST", "ApplyFeeOnShortCalls", false);

        fulldebug = config.GetValue<bool>("COST", "FullDebug", false);
        if (fulldebug) {
            basicdebug = true;
        }
        else {
            basicdebug = config.GetValue<bool>("COST", "BasicDebug", true);
        }

        Tick += OnTick;
        Interval = 50;

        UI.Notify("PhoneBills loaded."); //debug for when reloading scripthookdotnet
    }

    public void PhoneBillsCounterStart() { //get the call start time
        startcalltime = ((GTA.Native.Function.Call<int>(GTA.Native.Hash.GET_CLOCK_HOURS) * 60) + GTA.Native.Function.Call<int>(GTA.Native.Hash.GET_CLOCK_MINUTES));
        startcalltimegenerated = true;
    }

    void OnTick(object sender, EventArgs e) {
        Ped player = Game.Player.Character;

        usingphone = GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_MOBILE_PHONE_CALL_ONGOING); //is the player actually calling?
        //usingphone = GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_PED_RUNNING_MOBILE_PHONE_TASK, player); //is applied when the player just picks the phone

        if (usingphone) {
            usedphone = true;
            if (!startcalltimegenerated) {
                PhoneBillsCounterStart();
            }
            if (fulldebug) {
                UI.ShowSubtitle("Calling...");
            }
        }

        if (usedphone && !usingphone) {
            calltime = ((GTA.Native.Function.Call<int>(GTA.Native.Hash.GET_CLOCK_HOURS) * 60) + GTA.Native.Function.Call<int>(GTA.Native.Hash.GET_CLOCK_MINUTES)) - startcalltime;
            //int finalcallcost = calltime * callcost;
            int finalcallcost = (calltime * callcost) + feecost;
            if (finalcallcost < mincallcost) { //charge the min. call cost
                finalcallcost = mincallcost;
            }
            if (finalcallcost > maxcallcost) { //dont charge more than the max. call cost
                finalcallcost = maxcallcost;
            }
            if (freeshortcalls && calltime <= freeshortcallsmaxtime) { //if short calls are free
                if (feecostonshortcalls) { //if free calls really cost the fee
                    finalcallcost = feecost;
                }
                else { //if free calls are totally free - no fee on short calls
                    finalcallcost = 0;
                }
            }
            Wait(2000);
            Game.Player.Money -= finalcallcost; //extract money -final call cost- from player
            if (basicdebug) {
                UI.ShowSubtitle("Call time: " + calltime + " seconds. Call cost: " + finalcallcost + " $" );
            }
            usedphone = false;
            startcalltimegenerated = false;
        }
    }
}