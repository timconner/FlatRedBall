using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using FlatRedBall.Glue.Managers;
using FlatRedBall.Glue.Plugins.ExportedInterfaces.CommandInterfaces;
using GameCommunicationPlugin.GlueControl.CommandSending;
using GameCommunicationPlugin.GlueControl.Dtos;
using Glue;

namespace GameCommunicationPlugin.GlueControl.Managers;

class ModalReportingService
{
    Timer modalOpenTimer;
    private ISynchronizeInvoke _synchronizingObject;
    private IDialogCommands _dialogCommands;
    private CommandSender _commandSender;

    public ModalReportingService(ISynchronizeInvoke synchronizingObject, IDialogCommands dialogCommands, CommandSender commandSender)
    {
        _synchronizingObject = synchronizingObject;
        _dialogCommands = dialogCommands;
        _commandSender = commandSender;
    }

    public void Initialize()
    {
        var frequency = 2_000; // ms
        modalOpenTimer = new Timer(frequency);
        modalOpenTimer.Elapsed += UpdateTimer;
        modalOpenTimer.SynchronizingObject = _synchronizingObject;
        modalOpenTimer.Start();
    }

    bool? lastModalSent = null;

    private void UpdateTimer(object sender, ElapsedEventArgs e)
    {
        var isModal = _dialogCommands.IsModalWindowOpen();
        Debug.WriteLine("Is modal open: " + isModal);

        if(lastModalSent != isModal)
        {
            lastModalSent = isModal;

            var dto = new GlueModalWindowStatusDto
            {
                IsModalWindowOpen = isModal
            };
            _ = _commandSender.Send(dto);
        }
    }
}
