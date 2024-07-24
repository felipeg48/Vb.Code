Imports System.Collections.Concurrent
Imports System.Threading

Public Class ExpiringCache
    Private _cache As New ConcurrentDictionary(Of Integer, (value As Object, expirationTime As DateTime))
    Private _expirationInterval As TimeSpan
    Private _cancellationTokenSource As CancellationTokenSource
    Private _cleanupTask As Task

    Public Sub New(Optional expirationInterval As TimeSpan = Nothing)
        _expirationInterval = If(expirationInterval = Nothing, TimeSpan.FromMinutes(25), expirationInterval)
        _cancellationTokenSource = New CancellationTokenSource()
        _cleanupTask = StartCleanupTask() 
    End Sub

    Public Sub Add(key As Integer, value As Object)
        Dim expirationTime = DateTime.Now.Add(_expirationInterval)
        _cache(key) = (value, expirationTime)
    End Sub

    Public Function TryGetValue(key As Integer, ByRef value As Object) As Boolean
        Dim entry As (value as Object, expirationTime as Date) = Nothing
        If _cache.TryGetValue(key, entry) Then
            If entry.expirationTime > DateTime.Now Then
                value = entry.value
                Return True
            Else
                _cache.TryRemove(key, Nothing) ' Remove expired entry
            End If
        End If
        Return False
    End Function

    Private Async Function StartCleanupTask() As Task
        Do While Not _cancellationTokenSource.Token.IsCancellationRequested
            Try
                Await Task.Delay(_expirationInterval, _cancellationTokenSource.Token)
                CleanupExpiredItems()
            Catch ex As TaskCanceledException
                Exit Do ' Exit loop if cancellation is requested
            End Try
        Loop
    End Function

    Public Async Sub StopCleanupTask()
        _cancellationTokenSource.Cancel()
        Await _cleanupTask
    End Sub

    Private Sub CleanupExpiredItems()
        Dim now = DateTime.Now
        For Each entry In _cache
            If entry.Value.expirationTime <= now Then
                _cache.TryRemove(entry.Key, Nothing)
            End If
        Next
    End Sub
End Class
