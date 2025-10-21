#region Using directives
using System;
using CoreBase = FTOptix.CoreBase;
using FTOptix.HMIProject;
using UAManagedCore;
using FTOptix.UI;
using FTOptix.NetLogic;
using FTOptix.OPCUAServer;
using FTOptix.Core;
using System.Linq;
using FTOptix.WebUI;
#endregion


public class PeriodicTasks : BaseNetLogic
{
	public override void Start()
	{
		//periodicTask = new PeriodicTask(UpdatePasswordOld, 5000, LogicObject);
		periodicExpertPassword = new PeriodicTask(UpdatePasswordNew, 2000, LogicObject);
		periodicDongleCheck = new PeriodicTask(CheckForUSBDongle, 2000, LogicObject);
		periodicExpertTimeout = new PeriodicTask(CheckExpertTimeout, 2000, LogicObject);

		periodicExpertPassword.Start();
		periodicDongleCheck.Start();
		periodicExpertTimeout.Start();

		//Remove expert level from expert key at startup!
		RemoveExpertTemp();
	}

	public override void Stop()
	{
		periodicExpertPassword.Dispose();
		periodicExpertPassword = null;
		periodicDongleCheck.Dispose();
		periodicDongleCheck = null;
		periodicExpertTimeout.Dispose();
		periodicExpertTimeout = null;
	}

	private void UpdatePasswordOld()
	{
		DateTime localTime = DateTime.Now;
		LogicObject.GetVariable("Time").Value = localTime;
		LogicObject.GetVariable("Time/Year").Value = localTime.Year;
		LogicObject.GetVariable("Time/Month").Value = localTime.Month;
		LogicObject.GetVariable("Time/Day").Value = localTime.Day;
		LogicObject.GetVariable("Time/Hour").Value = localTime.Hour;

		int Hour = localTime.Hour;
		int Day = localTime.Day;
		int Month = localTime.Month;
		int Year = localTime.Year % 100;

		if (Year <= 50)
		{
			Year = 99 - Year;
		}
		int Passwordtemp = Day + Month + Year + Hour;
		Passwordtemp = Passwordtemp * Hour * Day * Month * Year;
		if (Passwordtemp > 99999999)
		{
			Passwordtemp = Passwordtemp / 10;
		}
		//LogicObject.GetVariable("PasswordExpert").Value = Passwordtemp;

	}

	private void UpdatePasswordNew()
	{
		
		//Directly Set pw of User Rovema
		Session.ChangePasswordInternal("Rovema", CalcTempPassword());

	}

	private string CalcTempPassword()
	{
		DateTime localTime = DateTime.Now;
		LogicObject.GetVariable("Time").Value = localTime;
		LogicObject.GetVariable("Time/Year").Value = localTime.Year;
		LogicObject.GetVariable("Time/Month").Value = localTime.Month;
		LogicObject.GetVariable("Time/Day").Value = localTime.Day;
		LogicObject.GetVariable("Time/Hour").Value = localTime.Hour;

		int Hour = localTime.Hour + 2; // [2-25]
		int Day = localTime.Day + 1; // [2-32]
		int Month = localTime.Month + 1; // [2-13]
		int Year = localTime.Year;

		int HourlyPass = (((Hour << 6) + 1) | Day) ^ (((Month << 4) - 1) | (Year % 2218));
		HourlyPass += Hour + Day + Year + Month;
		HourlyPass += Hour * Day * Month;

		while (HourlyPass < 5555555)
		{
			HourlyPass *= ((Day + Year + Hour) % 100) + 2;
			if (HourlyPass % 10 == 0)
				HourlyPass += 1;
		}
		return HourlyPass.ToString();
	}

	private void CheckForUSBDongle()
	{
		//TBD
	}

	private void CheckExpertTimeout()
	{
		bool loggedIn = Session.LoggedIn;
		if (loggedIn)
		{
			DateTime localTime = DateTime.Now;
			var ExpertLoginTimestamp = LogicObject.GetVariable("ExpertLogInTimeStamp").Value;
			var ExpertTime = localTime - ExpertLoginTimestamp;
			// 1 Hour in microseconds
			if (ExpertTime > TimeSpan.FromHours(1))
			{
				//remove expert from current user
				RemoveExpertTemp();

			}
		}
	}


	[ExportMethod]
	public void ExpertLogin()
	{
		// Insert code to be executed by the method
		// Panelloader main to open close the corresponding dialogwindows
		PanelLoader panelLoader = InformationModel.Get<PanelLoader>(LogicObject.GetVariable("PanelloaderMain").Value);
		//check for active user
		bool loggedIn = Session.LoggedIn;
		if (!loggedIn)
		{
			//close window
			foreach (Dialog item in Session.Get("UIRoot").Children.OfType<Dialog>().ToList())
			{
				item.Close();
			}
			// Source type definition of the DialogBox
			var DialogSessionError = (DialogType)Project.Current.Get("UI/Component library/Page/Miscellaneous/UserAdministration/LoginFailedNoSession");
			// DialogBoxes needs a graphical container as parent in order to
			// understand to which session they have to spawn
			UICommands.OpenDialog(panelLoader, DialogSessionError);
			return;
		}

		//Get input of password
		var DialogBoxNodeId = LogicObject.GetVariable("ExpertInput").Value;
		IUANode DialogBox = InformationModel.Get(DialogBoxNodeId);
		var UserInputList = DialogBox.FindNodesByType<TextBox>();

		string UserInput = "";
		foreach (Dialog item in Session.Get("UIRoot").Children.OfType<Dialog>().ToList()) {
			if (item.BrowseName.Contains("ExpertMode")) {
				foreach (var textBox in item.FindNodesByType<TextBox>())
				{
					if (textBox.BrowseName.Contains("ExpertKeyInput"))
					{
						UserInput = textBox.Text;
					}
				}
			}
		}
		
	
		string expertPW = CalcTempPassword();

		if (UserInput == expertPW)
		{

			// Add Expert_temp to active user
			var ExpertTempGroup = Project.Current.Get("Security/Groups/Expert_temp");

			var ActiveUser = Session.User;
			ActiveUser.Refs.AddReference(FTOptix.Core.ReferenceTypes.HasGroup, ExpertTempGroup.NodeId);

			// safe Timestamp 
			DateTime localTime = DateTime.Now;
			LogicObject.GetVariable("ExpertLogInTimeStamp").Value = localTime;

			// close modal window(s)
			foreach (Dialog item in Session.Get("UIRoot").Children.OfType<Dialog>().ToList())
			{
				item.Close();
			}

			// FTOptix User has to relog in order for the new Group to take effect

			//Session.Logout();
			//Session.Login(ActiveUser.BrowseName, "");
		}
		else
		{
			//close window
			foreach (Dialog item in Session.Get("UIRoot").Children.OfType<Dialog>().ToList())
			{
				item.Close();
			}
			//Open error for wrong expert key
			var DialogPasswordError = (DialogType)Project.Current.Get("UI/Component library/Page/Miscellaneous/UserAdministration/LoginFailedPassword");
			UICommands.OpenDialog(panelLoader, DialogPasswordError);
			return;
		}

		

	}

	[ExportMethod]
	public void RemoveExpertTemp()
	{
		Folder userFolder = Project.Current.Get<Folder>("Security/Users");
		var ExpertTempGroup = Project.Current.Get("Security/Groups/Expert_temp");

		foreach (var user in userFolder.Children.OfType<User>())
		{
			// Get all groups the user is part of
			var userGroups = user.Refs.GetObjects(FTOptix.Core.ReferenceTypes.HasGroup, false);

			// Iterate through the groups to check if the user is part of Expert_temp
			foreach (var userGroup in userGroups)
			{
				if (userGroup.NodeId == ExpertTempGroup.NodeId)
				{
					user.Refs.RemoveReference(FTOptix.Core.ReferenceTypes.HasGroup, ExpertTempGroup.NodeId);
				}
			}
		}

	}

	private PeriodicTask periodicExpertPassword;
	private PeriodicTask periodicDongleCheck;
	private PeriodicTask periodicExpertTimeout;
	private string ExpertPassword;
}

