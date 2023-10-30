using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AmbientSounds.Services;

public interface IQuickResumeService
{
    Task<bool> TryEnableAsync();

    void Disable();

    void SendQuickResumeToast();
}
