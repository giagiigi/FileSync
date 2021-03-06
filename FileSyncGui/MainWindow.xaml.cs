﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;

using FolderPickerLib;

using FileSyncObjects;
using FileSyncGui.Ref;

namespace FileSyncGui {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged {

		public Credentials credentials; //is set from login window

		private UserContents user;

		public MachineContents machine; //is set from machine window

		private FileSyncConnection connection;

		private FileSyncLocal local;

		private DispatcherTimer dt;

		public event PropertyChangedEventHandler PropertyChanged;

		private Boolean loggedIn = false;
		public Boolean LoggedIn {
			get { return loggedIn; }
			set {
				if (loggedIn == value)
					return;
				loggedIn = value;

				if (!loggedIn) {
					SelectedFile = false;
					SelectedDirectory = false;
					SelectedMachine = false;
					LoggedInAndChosenMachine = false;
				}

				if (PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs("LoggedIn"));
			}
		}

		private Boolean selectedMachine = false;
		public Boolean SelectedMachine {
			get { return selectedMachine; }
			set {
				if (selectedMachine == value)
					return;
				selectedMachine = value;

				if (PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs("SelectedMachine"));
			}
		}

		private Boolean selectedDirectory = false;
		public Boolean SelectedDirectory {
			get { return selectedDirectory; }
			set {
				if (selectedDirectory == value)
					return;
				selectedDirectory = value;

				if (PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs("SelectedDirectory"));
			}
		}

		private int selectedDirectoryIndex = -1;
		public int SelectedDirectoryIndex {
			get { return selectedDirectoryIndex; }
			set {
				if (selectedDirectoryIndex == value)
					return;
				selectedDirectoryIndex = value;

				if (PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs("SelectedDirectoryIndex"));
			}
		}

		private Boolean selectedFile = false;
		public Boolean SelectedFile {
			get { return selectedFile; }
			set {
				if (selectedFile == value)
					return;
				selectedFile = value;

				if (PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs("SelectedFile"));
			}
		}

		private Boolean loggedInAndChosenMachine = false;
		public Boolean LoggedInAndChosenMachine {
			get { return loggedInAndChosenMachine; }
			set {
				if (loggedInAndChosenMachine == value)
					return;
				loggedInAndChosenMachine = value;

				if (PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs("LoggedInAndChosenMachine"));
			}
		}

		private Boolean duringSync = false;
		public Boolean DuringSync {
			get { return duringSync; }
			set {
				if (duringSync == value)
					return;
				duringSync = value;

				if (PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs("DuringSync"));
			}
		}

		private Boolean duringNetworkActions = false;
		public Boolean DuringNetworkActions {
			get { return duringNetworkActions; }
			set {
				if (duringNetworkActions == value)
					return;
				duringNetworkActions = value;

				if (PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs("DuringNetworkActions"));
			}
		}

		private String machineName = "name";
		public String MachineName {
			get { return machineName; }
			set {
				if (machineName == value)
					return;
				machineName = value;

				if (PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs("MachineName"));
			}
		}

		private String machineDesc = "desc";
		public String MachineDesc {
			get { return machineDesc; }
			set {
				if (machineDesc == value)
					return;
				machineDesc = value;

				if (PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs("MachineDesc"));
			}
		}

		private ObservableCollection<DirectoryContents> directories;
		public ObservableCollection<DirectoryContents> Directories {
			get { return directories; }
		}

		public MainWindow() {
			//this.DataContext = this;

			connection = new FileSyncConnection();
			local = new FileSyncLocal();

			LoggedIn = false;
			LoggedInAndChosenMachine = false;
			SelectedMachine = false;
			SelectedDirectory = false;
			SelectedFile = false;
			DuringSync = false;
			DuringNetworkActions = false;
			directories = new ObservableCollection<DirectoryContents>();

			dt = new DispatcherTimer();
			dt.Interval = TimeSpan.FromMilliseconds(1000);
			dt.Tick += new EventHandler(dt_Tick);
			dt.IsEnabled = true;

			InitializeComponent();
		}

		public void dt_Tick(object sender, EventArgs e) {
			this.buttonLoginCreate_Click(sender, null);

			dt.IsEnabled = false;
		}

		private void RefreshDisplayedMachineInfo() {
			MachineName = machine.Name;
			MachineDesc = machine.Description;
			LoggedInAndChosenMachine = true;
		}

		private void RefreshDisplayedDirectories() {
			directories.Clear();
			foreach (DirectoryContents d in machine.Directories) {
				connection.GetDirectoryWithFiles(credentials, machine, d);
				local.ReadDirectoryContents(d);
				Directories.Add(d);
			}
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs("Directories"));
		}

		private void buttonLoginCreate_Click(object sender, RoutedEventArgs e) {
			LoginWindow loginWin = new LoginWindow(this);
			loginWin.Owner = this;
			if (loginWin.ShowDialog() == true) {
				if (this.credentials == null)
					return;
				try {

					this.user = connection.GetUserWithMachines(credentials);
					//new UserContents(credentials, true);
					LoggedIn = true;

					if (this.machine == null)
						buttonCreateSelectMachine_Click(sender, e);
					else
						RefreshDisplayedMachineInfo();
				} catch (ActionException ex) {
					new SystemMessage(ex).ShowDialog();
					//MessageBox.Show(ex.Message, ex.Title);
				}
			}
		}

		private void buttonCreateSelectMachine_Click(object sender, RoutedEventArgs e) {
			if (!LoggedIn)
				return;

			//temporary
			UserContents user = connection.GetUserWithMachines(credentials);
			//new UserContents(credentials, true).Machines;
			// UserActions.GetContents(this.credentials).Machines;
			this.machine = connection.GetMachineWithDirs(credentials, user.Machines[0]);
			//this.machine = user.Machines[0];
			//new MachineContents(credentials, machines[0], true, false, true);
			//MachineActions.GetContets(credentials, machines[0]);
			RefreshDisplayedMachineInfo();
			if (machine != null) return;
			//end of temporary

			if (new MachineWindow(this).ShowDialog() == false)
				return;

			if (this.machine == null) {
				LoggedInAndChosenMachine = false;
				return;
			}

			RefreshDisplayedMachineInfo();

			try {
				//foreach (DirContents dc in MachineActions.GetDirsContents(credentials, machine))
				//	directories.Add(dc);
				//if (PropertyChanged != null)
				//    PropertyChanged(this, new PropertyChangedEventArgs("Directories"));

			} catch (ActionException ex) {
				MessageBox.Show(ex.Message, ex.Title);
			}

			RefreshDisplayedDirectories();
		}

		private void buttonChangeMachine_Click(object sender, RoutedEventArgs e) {
			buttonCreateSelectMachine_Click(sender, e);
		}

		private void buttonLogout_Click(object sender, RoutedEventArgs e) {
			LoggedIn = false;

			credentials = null;
			user = null;
			machine = null;
		}

		private void optionExit_Click(object sender, RoutedEventArgs e) {
			this.Close();
		}

		private void optionSelectMachine_Click(object sender, RoutedEventArgs e) {
			buttonCreateSelectMachine_Click(sender, e);
		}

		private void optionCreateMachine_Click(object sender, RoutedEventArgs e) {
			buttonCreateSelectMachine_Click(sender, e);
		}

		private void optionAddDir_Click(object sender, RoutedEventArgs e) {
			buttonAddDir_Click(sender, e);
		}

		private void buttonAddDir_Click(object sender, RoutedEventArgs e) {
			var pickedFolder = new FolderPickerDialog();
			try {
				if (pickedFolder.ShowDialog() == true) {
					string path = pickedFolder.SelectedPath;
					if (path == null)
						throw new ActionException("No folder was chosen.", ActionType.Directory);
					//DirIdentity did = new DirIdentity(path.Substring(path.LastIndexOf('\\') + 1),
					//	null, path);
					DirectoryContents dcNew = new DirectoryContents(
						path.Substring(path.LastIndexOf('\\') + 1), path);
					//path.Substring(path.LastIndexOf('\\') + 1), path, null, null, true);
					dcNew = local.ReadDirectoryContents(dcNew);
					foreach (DirectoryContents dc in Directories)
						if (dc.Name.Equals(dcNew.Name) || dc.LocalPath.Equals(dcNew.LocalPath))
							throw new ActionException("Folder already exists on this machine.",
								ActionType.Directory);
					Directories.Add(dcNew);
					machine.Directories.Add(dcNew);
				}
			} catch (ActionException ex) {
				MessageBox.Show(ex.Message, ex.Title);
			}
		}

		private void buttonSaveChanges_Click(object sender, RoutedEventArgs e) {
			try {
				//MessageBox.Show(String.Format("old: {0} new: {1} {2}", machine.Identity.ToString(),
				//	MachineName, MachineDesc), "Erroren");

				var newMachine = new MachineContents(MachineName, MachineDesc);

				if (connection.ChangeMachineDetails(credentials, newMachine, machine)) {
					machine.Name = MachineName;
					machine.Description = MachineDesc;
					new SystemMessage("FileSync", "Changes saved",
						"Machine metadata was successfully updated.", MemeType.FuckYea).ShowDialog();
				} else
					new SystemMessage("FileSync", "Error", "bool WCF function returned false").ShowDialog();

			} catch (ActionException ex) {
				MessageBox.Show(ex.Message, ex.Title);
			}
		}

		private void buttonCancel_Click(object sender, RoutedEventArgs e) {
			RefreshDisplayedMachineInfo();
		}

		private void buttonDelete_Click(object sender, RoutedEventArgs e) {
			optionDeleteMachine_Click(sender, e);
		}

		private void optionChangePassword_Click(object sender, RoutedEventArgs e) {
			MessageBox.Show("no handler launched");
		}

		private void optionResetPassword_Click(object sender, RoutedEventArgs e) {
			MessageBox.Show("no handler launched");
		}

		private void optionUploadMachine_Click(object sender, RoutedEventArgs e) {
			try {
				if (local.UploadMachine(connection, credentials, machine))
					new SystemMessage("FileSync", "Upload finished",
						"Machine was uploaded successfully.", MemeType.FuckYea).ShowDialog();
				else {
					new SystemMessage("FileSync", "Not good...",
							"Machine was not uploaded successfully.").ShowDialog();
				}
				//MessageBox.Show("Backup complete.", "FileSync",
				//    MessageBoxButton.OK, MessageBoxImage.Information);
			} catch (ActionException ex) {
				new SystemMessage(ex).ShowDialog();
				//MessageBox.Show(ex.Message, ex.Title);
			}
		}

		private void optionDownloadMachine_Click(object sender, RoutedEventArgs e) {

			try {
				local.DownloadMachine(connection, credentials, machine);
				local.ReadMachineContents(machine, true);

				//machine = connection(credentials, machine);
				//machine = new MachineContents(credentials, machine, true, true, true);
				//MachineActions.GetContets(credentials, machine.Identity);

				RefreshDisplayedMachineInfo();

				if (local.SaveMachine(machine))
					new SystemMessage("FileSync", "File sync complete.",
						"All directories defined in this machine were downloaded and restored.",
						MemeType.FuckYea).ShowDialog();
				else new SystemMessage("FileSync", "This is embarassing.",
					"The machine was not downloaded successfully.").ShowDialog();

				//foreach (DirIdentity did in machine.Directories) {
				//    var files = DirActions.Download(credentials, machine.Identity, did);
				//    foreach (FileContents f in files)
				//        f.SaveToDisk(did);
				//}

				//MessageBox.Show("Directories restored.", "FileSync",
				//    MessageBoxButton.OK, MessageBoxImage.Information);
			} catch (ActionException ex) {
				new SystemMessage(ex).ShowDialog();
				//MessageBox.Show(ex.Message, ex.Title);
			}

		}

		private void optionDeleteMachine_Click(object sender, RoutedEventArgs e) {
			try {
				//connection.Del
				//machine.DeleteFromDatabase(credentials);
				//MachineActions.Delete(credentials, machine.Identity);
			} catch (ActionException ex) {
				new SystemMessage(ex).ShowDialog();
				//MessageBox.Show(ex.Message, ex.Title);
			}
		}

		private void optionUploadDir_Click(object sender, RoutedEventArgs e) {
			try {
				if (SelectedDirectoryIndex < 0)
					throw new ActionException("No directory was selected to upload.",
						ActionType.Directory);

				DirectoryContents dc = machine.Directories[SelectedDirectoryIndex];

				local.UploadDirectory(connection, credentials, machine, dc);

				new SystemMessage("FileSync", "Upload complete",
					"Directory was uploaded successfully", MemeType.FuckYea).ShowDialog();
				//DirActions.Upload(credentials, machine.Identity, did, new DirContents(did, true));

				//MessageBox.Show("Backup of the selected directory complete.", "FileSync",
				//	MessageBoxButton.OK, MessageBoxImage.Information);
			} catch (ActionException ex) {
				new SystemMessage(ex).ShowDialog();
				//MessageBox.Show(ex.Message, ex.Title);
			}

		}

		private void optionDownloadDir_Click(object sender, RoutedEventArgs e) {
			try {
				if (SelectedDirectoryIndex < 0)
					throw new ActionException("No directory was selected to download.",
						ActionType.Directory);

				DirectoryIdentity did = machine.Directories[SelectedDirectoryIndex];
				//var dc = new DirectoryContents(credentials, machine, did, true, true);
				DirectoryContents d = machine.Directories[SelectedDirectoryIndex];
				local.DownloadDirectory(connection, credentials, machine, d);//...GetFileList();
				if (local.SaveDirectory(d)) {
					new SystemMessage("FileSync", "File sync successful.",
						"Selected directory was downloaded and restored.", MemeType.FuckYea)
						.ShowDialog();
				}

				//var files = DirActions.Download(credentials, machine.Identity, did);
				//foreach (FileContents f in files)
				//	f.SaveToDisk(did);

				RefreshDisplayedDirectories();

				//MessageBox.Show("Directory restored.", "FileSync",
				//    MessageBoxButton.OK, MessageBoxImage.Information);
			} catch (ActionException ex) {
				new SystemMessage(ex).ShowDialog();
				//MessageBox.Show(ex.Message, ex.Title);
			}
		}

		private void optionModifyRights_Click(object sender, RoutedEventArgs e) {
			MessageBox.Show("no handler launched");

			//DirActions.ModifyRights();
		}

		private void optionDeleteDir_Click(object sender, RoutedEventArgs e) {

			try {
				//connection.Del
				//Directories[SelectedDirectoryIndex].DeleteFromDatabase(credentials, machine);
			} catch (ActionException ex) {
				new SystemMessage(ex).ShowDialog();
				//MessageBox.Show(ex.Message, ex.Title);
			}
		}

		private void optionAddFile_Click(object sender, RoutedEventArgs e) {
			MessageBox.Show("no handler launched");
		}

		private void optionRemoveFile_Click(object sender, RoutedEventArgs e) {
			MessageBox.Show("no handler launched");
		}

		private void optionUploadFile_Click(object sender, RoutedEventArgs e) {

			try {
				int dirIndex = 0; //TODO
				int fileIndex = 0; //TODO

				var dc = machine.Directories[dirIndex];
				var fc = Directories[dirIndex].Files[fileIndex];

				if (!connection.AddFile(credentials, machine, dc, fc))
					new SystemMessage("FileSync", "Error", "bool WCF function returned false").ShowDialog();

				//fc.Upload(credentials, machine, dc);
				//FileActions.Upload(credentials, machine.Identity, did, fid,
				//	new FileContents(did.LocalPath + "\\" + fid.Name));
			} catch (ActionException ex) {
				MessageBox.Show(ex.Message, ex.Title);
			}
		}

		private void optionDownloadFile_Click(object sender, RoutedEventArgs e) {
			try {
				int dirIndex = 0; //TODO
				int fileIndex = 0; //TODO

				var did = machine.Directories[dirIndex];
				var fid = Directories[dirIndex].Files[fileIndex];
				//var newFile = new FileContents(credentials, machine, did, fid);
				//directories[dirIndex].Files[fileIndex].
				//FileActions.Download(credentials, machine.Identity, did, fid);
			} catch (ActionException ex) {
				MessageBox.Show(ex.Message, ex.Title);
			}
		}

		private void optionViewMetadata_Click(object sender, RoutedEventArgs e) {
			MessageBox.Show("no handler launched");
		}

		private void optionChangeType_Click(object sender, RoutedEventArgs e) {
			MessageBox.Show("no handler launched");
		}

		private void optionDeleteFile_Click(object sender, RoutedEventArgs e) {
			MessageBox.Show("no handler launched");
		}

		private void MachineDirsTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
			if (e.NewValue == null)
				return;

			if (e.NewValue.GetType() == typeof(DirectoryContents)) {
				SelectedDirectory = true;
				SelectedDirectoryIndex = Directories.IndexOf((DirectoryContents)e.NewValue);
				SelectedFile = false;
				//SelectedFileIndex = -1;
			} else if (e.NewValue.GetType() == typeof(FileIdentity)) {
				SelectedDirectory = false;
				SelectedDirectoryIndex = -1;
				SelectedFile = true;
			} else {
				SelectedDirectory = false;
				SelectedDirectoryIndex = -1;
				SelectedFile = false;
				//SelectedFileIndex = -1;
			}
		}
	}
}
