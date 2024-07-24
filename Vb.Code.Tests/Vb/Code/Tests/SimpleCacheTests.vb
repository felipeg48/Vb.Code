Imports Xunit

Namespace Vb.Code.Tests
    Public Class SimpleCacheTests
        <Fact>
        Sub TestSub()
            Dim cache = New SimpleCache.ExpiringCache(TimeSpan.FromSeconds(2))
            cache.Add(1, "one")
            cache.Add(2, "two")
            cache.Add(3, "three")
            
            Dim value As Object = Nothing
            Assert.True(cache.TryGetValue(1, value))
            Assert.Equal("one", value)

            Threading.Thread.Sleep(3000)
            Assert.False(cache.TryGetValue(1, value))
            
            cache.StopCleanupTask()
        End Sub
    End Class
End Namespace

