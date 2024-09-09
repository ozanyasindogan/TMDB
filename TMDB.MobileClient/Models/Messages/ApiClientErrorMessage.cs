using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace TMDB.MobileClient.Models.Messages;

public record ErrorMessage(string Title, string Message, Exception? RelatedException = null);

public class ApiClientErrorMessage(ErrorMessage errorMessage) 
    : ValueChangedMessage<ErrorMessage>(errorMessage);